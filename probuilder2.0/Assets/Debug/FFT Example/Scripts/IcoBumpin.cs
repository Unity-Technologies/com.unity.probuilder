using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.Math;

namespace ProBuilder2.Examples
{

	[RequireComponent(typeof(AudioSource))]
	public class IcoBumpin : MonoBehaviour
	{
		pb_Object ico;			// A reference to the icosphere pb_Object component
		Mesh icoMesh;
		Transform icoTransform;

		struct FaceRef
		{
			public pb_Face face;
			public Vector3 nrm;		// face normal
			public int[] indices;	// all vertex indices (including shared connected vertices)

			public FaceRef(pb_Face f, Vector3 n, int[] i)
			{
				face = f;
				nrm = n;
				indices = i;
			}
		}

		FaceRef[] outsides;
		Vector3[] original_vertices, displaced_vertices;

		[Range(1f, 10f)]
		public float icoRadius = 2f;

		[Range(0, 3)]
		public int icoSubdivisions = 2;

		[Range(0f, 1f)]
		public float startingExtrusion = .1f;

		public Material material;

		[Range(1f, 50f)]
		public float extrusion = 30f;

		[Range(8, 128)]
		public int fftBounds = 32;
		AudioSource audioSource;

		[Range(0f, 10f)]
		public float verticalBounce = 4f;

		public AnimationCurve lowEndCompensation;

		public LineRenderer waveform;
		public float waveformHeight	= 2f;
		public float waveformRadius	= 20f;
		public float waveformSpeed = .1f;
		public bool rotateWaveformRing = false;
		public bool bounceWaveform = false;

		Vector3 icoPosition = Vector3.zero;

		const float TWOPI = 6.283185f;	

		float faces_length;
		const int WAVEFORM_SAMPLES = 1024;
		const int FFT_SAMPLES = 4096;

		float[] fft = new float[FFT_SAMPLES],
				fft_history = new float[FFT_SAMPLES],
				data = new float[WAVEFORM_SAMPLES],
				data_history = new float[WAVEFORM_SAMPLES];

		float rms = 0f, rms_history = 0f;	// root mean square of raw data

		void Start()
		{
			audioSource = GetComponent<AudioSource>();

			ico = pb_Shape_Generator.IcosahedronGenerator(icoRadius, icoSubdivisions);
			pb_Face[] shell = ico.faces;
			
			foreach(pb_Face f in shell)
				f.SetMaterial( material );

			pb_Face[] connectingFaces;
			ico.Extrude(shell, startingExtrusion, false, out connectingFaces);

			ico.ToMesh();
			ico.Refresh();

			outsides = new FaceRef[shell.Length];
			Dictionary<int, int> lookup = ico.sharedIndices.ToDictionary();

			for(int i = 0; i < shell.Length; ++i)
				outsides[i] = new FaceRef( 	shell[i],
											pb_Math.Normal(ico, shell[i]),
											ico.sharedIndices.AllIndicesWithValues(lookup, shell[i].distinctIndices).ToArray()
											);

			original_vertices = new Vector3[ico.vertices.Length];
			System.Array.Copy(ico.vertices, original_vertices, ico.vertices.Length);

			displaced_vertices = ico.vertices;

			icoMesh = ico.msh;
			icoTransform = ico.transform;

			faces_length = (float)outsides.Length;

			icoPosition = icoTransform.position;
			waveform.SetVertexCount(WAVEFORM_SAMPLES);

			if( bounceWaveform )
				waveform.transform.parent = icoTransform;

			audioSource.Play();
		}

		void Update()
		{
			audioSource.GetSpectrumData(fft, 0, FFTWindow.BlackmanHarris);
			audioSource.GetOutputData(data, 0);
			rms = RMS(data);

			/**
			 * For each face, translate the vertices some distance
			 */
			for(int i = 0; i < outsides.Length; i++)
			{
				float normalizedIndex = (i/faces_length);

				int n = (int)(normalizedIndex*fftBounds);

				Vector3 displacement = outsides[i].nrm * ( ((fft[n]+fft_history[n]) * .5f) * (lowEndCompensation.Evaluate(normalizedIndex) * .5f + .5f)) * extrusion;

				foreach(int t in outsides[i].indices)
				{
					displaced_vertices[t] = original_vertices[t] + displacement;
				}
			}

			Vector3 vec = Vector3.zero;

			// Waveform ring
			for(int i = 0; i < WAVEFORM_SAMPLES; i++)
			{
				int n = i < WAVEFORM_SAMPLES-1 ? i : 0;
				vec.x = Mathf.Cos((float)n/WAVEFORM_SAMPLES * TWOPI) * (waveformRadius + (((data[n] + data_history[n]) * .5f) * waveformHeight));
				vec.z = Mathf.Sin((float)n/WAVEFORM_SAMPLES * TWOPI) * (waveformRadius + (((data[n] + data_history[n]) * .5f) * waveformHeight));

				vec.y = 0f;

				waveform.SetPosition(i, vec);
			}

			// Ring rotation
			if( rotateWaveformRing )
			{
				Vector3 rot = waveform.transform.localRotation.eulerAngles;

				rot.x = Mathf.PerlinNoise(Time.time * waveformSpeed, 0f) * 360f;
				rot.y = Mathf.PerlinNoise(0f, Time.time * waveformSpeed) * 360f;

				waveform.transform.localRotation = Quaternion.Euler(rot);
			}

			icoPosition.y = -verticalBounce + ((rms + rms_history) * verticalBounce);
			icoTransform.position = icoPosition;

			// Keep copy of last FFT samples so we can average with the current.  Smoothes the movement.
			System.Array.Copy(fft, fft_history, FFT_SAMPLES);
			System.Array.Copy(data, data_history, WAVEFORM_SAMPLES);
			rms_history = rms;

			icoMesh.vertices = displaced_vertices;
		}

		float RMS(float[] arr)
		{
			float 	v = 0f,
					len = (float)arr.Length;

			for(int i = 0; i < len; i++)
				v += Mathf.Abs(arr[i]);

			return Mathf.Sqrt(v / (float)len);
		}
	}
}