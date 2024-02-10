if [ -z "$1" ]
then
echo "No target Argument for nuget version"
else

sed -E -i "" "s/OpenTok.Net.webrtc.Dependency.Android\" version=\"([0-9]{1,}\.)+[0-9]{1,}/OpenTok.Net.webrtc.Dependency.Android\" version=\"$3/" NugetPackages/OpenTok.Net.mltransformers.Dependency.Android.nuspec

sed -E -i "" "s/OpenTok.Net.webrtc.Dependency.Android\" Version=\"([0-9]{1,}\.)+[0-9]{1,}/OpenTok.Net.webrtc.Dependency.Android\" Version=\"$3/" Bindings/OpenTok.Net.mltransformers.Dependency.Android/OpenTok.Net.mltransformers.Dependency.Android.csproj

sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$1.7/" Bindings/OpenTok.Net.mltransformers.Dependency.Android/OpenTok.Net.mltransformers.Dependency.Android.csproj

sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net7.0-android/" Bindings/OpenTok.Net.mltransformers.Dependency.Android/OpenTok.Net.mltransformers.Dependency.Android.csproj

dotnet pack Bindings/OpenTok.Net.mltransformers.Dependency.Android/OpenTok.Net.mltransformers.Dependency.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly

sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$1.8/" Bindings/OpenTok.Net.mltransformers.Dependency.Android/OpenTok.Net.mltransformers.Dependency.Android.csproj

sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net8.0-android/" Bindings/OpenTok.Net.mltransformers.Dependency.Android/OpenTok.Net.mltransformers.Dependency.Android.csproj

dotnet pack Bindings/OpenTok.Net.mltransformers.Dependency.Android/OpenTok.Net.mltransformers.Dependency.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly

cd NugetPackages
rm -rf mltransformers

unzip -n -q OpenTok.Net.mltransformers.Dependency.Android.$1.7.nupkg -d mltransformers
unzip -n -q OpenTok.Net.mltransformers.Dependency.Android.$1.8.nupkg -d mltransformers

rm OpenTok.Net.mltransformers.Dependency.Android.$1.7.nupkg
rm OpenTok.Net.mltransformers.Dependency.Android.$1.8.nupkg

# cp -R mltransformers/Readme.md mltransformers/Readme.txt

mkdir mltransformers/native
mkdir mltransformers/native/lib
mkdir mltransformers/native/lib/android
cp -R mltransformers/lib/net8.0-android34.0/mltransformers.aar mltransformers/native/lib/android

rm mltransformers/lib/net8.0-android34.0/mltransformers.aar
rm mltransformers/lib/net7.0-android33.0/mltransformers.aar 


sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$1.$2/" OpenTok.Net.mltransformers.Dependency.Android.nuspec

nuget pack OpenTok.Net.mltransformers.Dependency.Android.nuspec

rm -rf mltransformers

if  [ -z "$4" ]
then
echo "package ready"
unzip -n -q OpenTok.Net.mltransformers.Dependency.Android.$1.$2.nupkg -d mltransformers
else 
dotnet nuget push OpenTok.Net.mltransformers.Dependency.Android.$1.$2.nupkg --api-key $4 --source https://api.nuget.org/v3/index.json --timeout 3000000
# rm OpenTok.Net.mltransformers.Dependency.Android.$1.$2.nupkg
fi
cd ..
fi