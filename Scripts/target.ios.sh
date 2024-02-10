#!/bin/sh

if [ -z "$1" ]
then
echo "No target Argument for nuget version"
else
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$1.7/" Bindings/OpenTok.Net.iOS/OpenTok.Net.iOS.csproj

sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net7.0-ios/" Bindings/OpenTok.Net.iOS/OpenTok.Net.iOS.csproj

dotnet pack Bindings/OpenTok.Net.iOS/OpenTok.Net.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly

sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$1.8/" Bindings/OpenTok.Net.iOS/OpenTok.Net.iOS.csproj

sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net8.0-ios/" Bindings/OpenTok.Net.iOS/OpenTok.Net.iOS.csproj

dotnet pack Bindings/OpenTok.Net.iOS/OpenTok.Net.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly

cd NugetPackages
rm -rf ios

unzip -n -q OpenTok.Net.iOS.$1.7.nupkg -d ios
unzip -n -q OpenTok.Net.iOS.$1.8.nupkg -d ios

rm OpenTok.Net.iOS.$1.7.nupkg
rm OpenTok.Net.iOS.$1.8.nupkg

cp -R ios/Readme.md ios/Readme.txt
mkdir ios/native
mkdir ios/native/lib
mkdir ios/native/lib/ios

cp -R ios/lib/net8.0-ios17.2/OpenTok.Net.iOS.resources ios/native/lib/ios

rm -rf ios/lib/net8.0-ios17.2/OpenTok.Net.iOS.resources
rm -rf ios/lib/net7.0-ios16.1/OpenTok.Net.iOS.resources


sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$1.$2/" OpenTok.Net.iOS.nuspec

nuget pack OpenTok.Net.iOS.nuspec

rm -rf ios
cd ..

sed -E -i "" "s/OpenTok.Net.iOS\" Version=\"([0-9]{1,}\.)+[0-9]{1,}/OpenTok.Net.iOS\" Version=\"$1.$2/" SampleApps/MAUI.iOS/MAUI.iOS.csproj
cd SampleApps/MAUI.iOS
dotnet build -f net8.0-ios --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly

cd ../../NugetPackages

if  [ -z "$3" ]
then
echo "package ready"
unzip -n -q OpenTok.Net.iOS.$1.$2.nupkg -d ios
else 
dotnet nuget push OpenTok.Net.iOS.$1.$2.nupkg --api-key $3 --source https://api.nuget.org/v3/index.json --timeout 3000000
# rm OpenTok.Net.iOS.$1.$2.nupkg
fi
cd ..
fi