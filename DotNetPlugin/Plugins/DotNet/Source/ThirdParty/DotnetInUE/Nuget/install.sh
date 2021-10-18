#!/bin/bash

for file in *.nupkg
do
	./nuget.exe add "$file" -source .
	rm "$file"
done