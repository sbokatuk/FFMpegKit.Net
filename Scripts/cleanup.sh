#!/bin/sh


if [ -z "$1" ]
then
echo "for cleaning add parameter"
else

if [[ "$1" == "all" ]]
then
rm -rf Bindings
mkdir Bindings
rm -rf NugetPackages
mkdir NugetPackages
fi
if [[ "$1" == "nuget" ]]
then
rm -rf NugetPackages/*.nupkg
fi
fi



