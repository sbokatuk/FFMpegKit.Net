#!/bin/sh

cd NugetPackages
GLOBAL=$(ls -1 | grep 'nupkg' | awk '{print $1}');
GLOBALcount=$(ls -1 | grep 'nupkg' | awk '{print $1}' | wc -l | awk '{print $1}')
cd ..

echo "found for publish:"
echo "$GLOBAL"
for i in $(seq "$GLOBALcount")
do
currentNuget=$(echo "$GLOBAL" | sed -n ''$i' p');
dotnet nuget push NugetPackages/$currentNuget --api-key $1 --source https://api.nuget.org/v3/index.json --timeout 3000000
done