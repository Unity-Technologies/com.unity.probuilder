#!/bin/bash

# clean bin
if [ -d "bin" ]; then 
	rm -rf bin
fi

mkdir bin 

mcs src/*.cs -out:bin/pb-build.exe

cp bin/pb-build.exe ../pb-build.exe
