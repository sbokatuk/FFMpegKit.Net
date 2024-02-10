#!/bin/sh

for ARGUMENT in "$@"
do
   KEY=$(echo $ARGUMENT | cut -f1 -d=)

   KEY_LENGTH=${#KEY}
   VALUE="${ARGUMENT:$KEY_LENGTH+1}"

   export "$KEY"="$VALUE"
done



if [ -z "$VERSION" ]
then
echo "No target Argument for nuget version"
else

./target.webrtc.sh $WEBRTC $BUILD
./target.mltransformers.sh $MLTRANSFORMERS $BUILD $WEBRTC.$BUILD
./target.android.sh $ANDROIDVERSION $BUILD $MLTRANSFORMERS.$BUILD $WEBRTC.$BUILD
./target.ios.sh $IOSVERSION $BUILD

cd NugetPackages
rm -rf andr
rm -rf ios

unzip -n -q OpenTok.Net.Android.$ANDROIDVERSION.$BUILD.nupkg -d andr
unzip -n -q OpenTok.Net.iOS.$IOSVERSION.$BUILD.nupkg -d ios

cp -R Readme.md Readme.txt

sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$VERSION.$BUILD/" OpenTok.Net.nuspec

sed -E -i "" "s/OpenTok.Net.webrtc.Dependency.Android\" version=\"([0-9]{1,}\.)+[0-9]{1,}/OpenTok.Net.webrtc.Dependency.Android\" version=\"$WEBRTC.$BUILD/" OpenTok.Net.nuspec
sed -E -i "" "s/OpenTok.Net.mltransformers.Dependency.Android\" version=\"([0-9]{1,}\.)+[0-9]{1,}/OpenTok.Net.mltransformers.Dependency.Android\" version=\"$MLTRANSFORMERS.$BUILD/" OpenTok.Net.nuspec

nuget pack OpenTok.Net.nuspec

rm -rf andr
rm -rf ios

rm -rf net

cd ..

sed -E -i "" "s/OpenTok.Net\" Version=\"([0-9]{1,}\.)+[0-9]{1,}/OpenTok.Net\" Version=\"$VERSION.$BUILD/" SampleApps/MAUI/OpenTok.csproj
cd SampleApps/MAUI
dotnet build -f net8.0-android --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly

dotnet build -f net8.0-ios --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd ../../NugetPackages


if  [ -z "$5" ]
then
echo "package ready"
unzip -n -q OpenTok.Net.$VERSION.$BUILD.nupkg -d net
else 
dotnet nuget push OpenTok.Net.$VERSION.$BUILD.nupkg --api-key $5 --source https://api.nuget.org/v3/index.json --timeout 3000000
# rm OpenTok.Net.$VERSION.$BUILD.nupkg
fi
fi