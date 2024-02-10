if [ -z "$1" ]
then
echo "No target Argument for nuget version"
else


sed -E -i "" "s/OpenTok.Net.webrtc.Dependency.Android\" version=\"([0-9]{1,}\.)+[0-9]{1,}/OpenTok.Net.webrtc.Dependency.Android\" version=\"$4/" NugetPackages/OpenTok.Net.Android.nuspec
sed -E -i "" "s/OpenTok.Net.mltransformers.Dependency.Android\" version=\"([0-9]{1,}\.)+[0-9]{1,}/OpenTok.Net.mltransformers.Dependency.Android\" version=\"$3/" NugetPackages/OpenTok.Net.Android.nuspec
sed -E -i "" "s/OpenTok.Net.webrtc.Dependency.Android\" Version=\"([0-9]{1,}\.)+[0-9]{1,}/OpenTok.Net.webrtc.Dependency.Android\" Version=\"$4/" Bindings/OpenTok.Net.Android/OpenTok.Net.Android.csproj
sed -E -i "" "s/OpenTok.Net.mltransformers.Dependency.Android\" Version=\"([0-9]{1,}\.)+[0-9]{1,}/OpenTok.Net.mltransformers.Dependency.Android\" Version=\"$3/" Bindings/OpenTok.Net.Android/OpenTok.Net.Android.csproj

sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$1.7/" Bindings/OpenTok.Net.Android/OpenTok.Net.Android.csproj

sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net7.0-android/" Bindings/OpenTok.Net.Android/OpenTok.Net.Android.csproj

dotnet pack Bindings/OpenTok.Net.Android/OpenTok.Net.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly

sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$1.8/" Bindings/OpenTok.Net.Android/OpenTok.Net.Android.csproj

sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net8.0-android/" Bindings/OpenTok.Net.Android/OpenTok.Net.Android.csproj

dotnet pack Bindings/OpenTok.Net.Android/OpenTok.Net.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly

cd NugetPackages
rm -rf andr

unzip -n -q OpenTok.Net.Android.$1.7.nupkg -d andr
unzip -n -q OpenTok.Net.Android.$1.8.nupkg -d andr

rm OpenTok.Net.Android.$1.7.nupkg
rm OpenTok.Net.Android.$1.8.nupkg

cp -R andr/Readme.md andr/Readme.txt

mkdir andr/native
mkdir andr/native/lib
mkdir andr/native/lib/android
cp -R andr/lib/net8.0-android34.0/opentok-android-sdk.aar andr/native/lib/android

rm andr/lib/net8.0-android34.0/opentok-android-sdk.aar
rm andr/lib/net7.0-android33.0/opentok-android-sdk.aar 


sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$1.$2/" OpenTok.Net.Android.nuspec

nuget pack OpenTok.Net.Android.nuspec

rm -rf andr
cd ..

sed -E -i "" "s/OpenTok.Net.Android\" Version=\"([0-9]{1,}\.)+[0-9]{1,}/OpenTok.Net.Android\" Version=\"$1.$2/" SampleApps/MAUI.Android/MAUI.Android.csproj
cd SampleApps/MAUI.Android
dotnet build -f net8.0-android --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly

cd ../../NugetPackages

if  [ -z "$5" ]
then
echo "package ready"
unzip -n -q OpenTok.Net.Android.$1.$2.nupkg -d andr
else 
dotnet nuget push OpenTok.Net.Android.$1.$2.nupkg --api-key $5 --source https://api.nuget.org/v3/index.json --timeout 3000000
# rm OpenTok.Net.Android.$1.$2.nupkg
fi
cd ..
fi