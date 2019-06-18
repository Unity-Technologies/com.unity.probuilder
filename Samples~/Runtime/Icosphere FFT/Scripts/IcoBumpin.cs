// Create a sphere, extrude all of it's faces, then animate the extruded faces with an audio source.

#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace ProBuilder.Examples
{
	[RequireComponent(typeof(AudioSource))]
	public class IcoBumpin : MonoBehaviour
	{
		// A reference to the ProBuilderMesh component
		ProBuilderMesh m_ProBuilderMesh;
		// A reference to the MeshFilter.sharedMesh
		Mesh m_UnityMesh;
		Transform m_Transform;
		AudioSource m_AudioSource;
		Vector3 m_StartingPosition = Vector3.zero;
		float m_FaceLength;
		const float k_TwoPi = 6.283185f;
		// How many samples make up the waveform ring.
		const int k_WaveformSampleCount = 1024;
		// How many samples are used in the FFT. More means higher resolution.
		const int k_FftSamples = 4096;

		// Keep copy of the last frame's sample data to average with the current when calculating
		// deformation amounts. Smooths the visual effect.
		int m_FrameIndex;

		float[][] m_FourierSamples = new float[2][]
		{
			new float[k_FftSamples],
			new float[k_FftSamples]
		};

		float[][] m_RawSamples = new float[2][]
		{
			new float[k_WaveformSampleCount],
			new float[k_WaveformSampleCount]
		};

		// Root mean square of raw data (volume, but not in dB).
		float[] m_Rms = new float[2];

		/// <summary>
		/// This is the container for each extruded column. We'll use it to apply offsets per-extruded face.
		/// </summary>
		struct ExtrudedSelection
		{
			/// <value>
			/// The direction in which to move this selection when animating.
			/// </value>
			public Vector3 normal;

			/// <value>
			/// All vertex indices (including common vertices). "Common" refers to vertices that share a position
			/// but remain discrete.
			/// </value>
			public List<int> indices;

			public ExtrudedSelection(ProBuilderMesh mesh, Face face)
			{
				indices = mesh.GetCoincidentVertices(face.distinctIndexes);
				normal = Math.Normal(mesh, face);
			}
		}

		// All faces that have been extruded
		ExtrudedSelection[] m_AnimatedSelections;

		// Keep a copy of the original vertex array to calculate the distance from origin.
		Vector3[] m_OriginalVertexPositions, m_DisplacedVertexPositions;

		// The radius of the sphere on instantiation.
		[Range(1f, 10f)]
		public float icoRadius = 2f;

		// The number of subdivisions to give the sphere.
		[Range(0, 3)]
		public int icoSubdivisions = 2;

		// How far along the normal should each face be extruded when at idle (no audio input).
		[Range(0f, 1f)]
		public float startingExtrusion = .1f;

		// The max distance a frequency range will extrude a face.
		[Range(1f, 50f)]
		public float extrusion = 30f;

		// An FFT returns a spectrum including frequencies that are out of human hearing range.
		// This restricts the number of bins used from the spectrum to the lower bounds.
		[Range(8, 128)]
		public int fftBounds = 32;

		// How high the sphere transform will bounce (sample volume determines height).
		[Range(0f, 10f)]
		public float verticalBounce = 4f;

		// Optionally weights the frequency amplitude when calculating extrude distance.
		public AnimationCurve frequencyCurve;

		// A reference to the line renderer that will be used to render the raw waveform.
		public LineRenderer waveform;

		// The y size of the waveform.
		public float waveformHeight = 2f;

		// How far from the sphere should the waveform be.
		public float waveformRadius = 20f;

		// If rotateWaveformRing is true, this is the speed it will travel.
		public float waveformSpeed = .1f;

		// If true, the waveform ring will randomly orbit the sphere.
		public bool rotateWaveformRing = false;

		// If true, the waveform will bounce up and down with the sphere.
		public bool bounceWaveform = false;

		public GameObject missingClipWarning;

		/// <summary>
		/// Creates the sphere and loads all the cache information.
		/// </summary>
		void Start()
		{
			m_AudioSource = GetComponent<AudioSource>();

			if (m_AudioSource.clip == null)
				missingClipWarning.SetActive(true);

			// Create a new sphere.
			m_ProBuilderMesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center, icoRadius, icoSubdivisions);

			// Assign the default material
			m_ProBuilderMesh.GetComponent<MeshRenderer>().sharedMaterial = BuiltinMaterials.defaultMaterial;

			// Shell is all the faces on the new sphere.
			var shell = m_ProBuilderMesh.faces;

			// Extrude all faces on the sphere by a small amount. The third boolean parameter
			// specifies that extrusion should treat each face as an individual, not try to group
			// all faces together.
			m_ProBuilderMesh.Extrude(shell, ExtrudeMethod.IndividualFaces, startingExtrusion);

			// ToMesh builds the mesh positions, submesh, and triangle arrays. Call after adding
			// or deleting vertices, or changing face properties.
			m_ProBuilderMesh.ToMesh();

			// Refresh builds the normals, tangents, and UVs.
			m_ProBuilderMesh.Refresh();

			m_AnimatedSelections = new ExtrudedSelection[shell.Count];

			// Populate the outsides[] cache. This is a reference to the tops of each extruded column, including
			// copies of the sharedIndices.
			for (int i = 0; i < shell.Count; ++i)
			{
				m_AnimatedSelections[i] = new ExtrudedSelection(m_ProBuilderMesh, shell[i]);
			}

			// Store copy of positions array un-modified
			m_OriginalVertexPositions = m_ProBuilderMesh.positions.ToArray();

			// displaced_vertices should mirror sphere mesh vertices.
			m_DisplacedVertexPositions = new Vector3[m_ProBuilderMesh.vertexCount];

			m_UnityMesh = m_ProBuilderMesh.GetComponent<MeshFilter>().sharedMesh;
			m_Transform = m_ProBuilderMesh.transform;

			m_FaceLength = (float) m_AnimatedSelections.Length;

			// Build the waveform ring.
			m_StartingPosition = m_Transform.position;

			waveform.positionCount = k_WaveformSampleCount;

			if (bounceWaveform)
				waveform.transform.parent = m_Transform;

			m_AudioSource.Play();
		}

		void Update()
		{
			int currentFrame = m_FrameIndex;
			int previousFrame = (m_FrameIndex + 1) % 2;

			// fetch the fft spectrum
			m_AudioSource.GetSpectrumData(m_FourierSamples[m_FrameIndex], 0, FFTWindow.BlackmanHarris);

			// get raw data for waveform
			m_AudioSource.GetOutputData(m_RawSamples[m_FrameIndex], 0);

			// calculate root mean square (volume)
			m_Rms[m_FrameIndex] = CalculateLoudness(m_RawSamples[m_FrameIndex]);

			// For each face, translate the vertices some distance depending on the frequency range assigned.
			for (int i = 0; i < m_AnimatedSelections.Length; i++)
			{
				float normalizedIndex = (i / m_FaceLength);

				int n = (int) (normalizedIndex * fftBounds);

				Vector3 displacement = m_AnimatedSelections[i].normal *
				                       (((m_FourierSamples[currentFrame][n] + m_FourierSamples[previousFrame][n]) * .5f) *
				                        (frequencyCurve.Evaluate(normalizedIndex) * .5f + .5f)) * extrusion;

				foreach (int t in m_AnimatedSelections[i].indices)
				{
					m_DisplacedVertexPositions[t] = m_OriginalVertexPositions[t] + displacement;
				}
			}

			Vector3 vec = Vector3.zero;

			// Waveform ring
			for (int i = 0; i < k_WaveformSampleCount; i++)
			{
				int n = i < k_WaveformSampleCount - 1 ? i : 0;
				float travel = waveformRadius + (m_RawSamples[currentFrame][n] + m_RawSamples[previousFrame][n]) * .5f * waveformHeight;
				vec.x = Mathf.Cos((float) n / k_WaveformSampleCount * k_TwoPi) * travel;
				vec.z = Mathf.Sin((float) n / k_WaveformSampleCount * k_TwoPi) * travel;
				vec.y = 0f;

				waveform.SetPosition(i, vec);
			}

			// Ring rotation
			if (rotateWaveformRing)
			{
				Vector3 rot = waveform.transform.localRotation.eulerAngles;

				rot.x = Mathf.PerlinNoise(Time.time * waveformSpeed, 0f) * 360f;
				rot.y = Mathf.PerlinNoise(0f, Time.time * waveformSpeed) * 360f;

				waveform.transform.localRotation = Quaternion.Euler(rot);
			}

			m_StartingPosition.y = -verticalBounce + ((m_Rms[currentFrame] + m_Rms[previousFrame]) * verticalBounce);
			m_Transform.position = m_StartingPosition;

			// Keep copy of last FFT samples so we can average with the current. Smoothes the movement.
			m_FrameIndex = (m_FrameIndex + 1) % 2;

			// Apply the new extruded vertex positions to the MeshFilter.
			m_UnityMesh.vertices = m_DisplacedVertexPositions;
		}

		/// <summary>
		/// Root mean square is a good approximation of perceived loudness.
		/// </summary>
		/// <param name="arr"></param>
		/// <returns></returns>
		static float CalculateLoudness(float[] arr)
		{
			float v = 0f,
				len = (float) arr.Length;

			for (int i = 0; i < len; i++)
				v += Mathf.Abs(arr[i]);

			return Mathf.Sqrt(v / (float) len);
		}
	}
}
#endif
