if [ -z "$1" ]
then
echo "No target Argument for nuget version"
else
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$1.7/" Bindings/OpenTok.Net.webrtc.Dependency.Android/OpenTok.Net.webrtc.Dependency.Android.csproj

sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net7.0-android/" Bindings/OpenTok.Net.webrtc.Dependency.Android/OpenTok.Net.webrtc.Dependency.Android.csproj

dotnet pack Bindings/OpenTok.Net.webrtc.Dependency.Android/OpenTok.Net.webrtc.Dependency.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly

sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$1.8/" Bindings/OpenTok.Net.webrtc.Dependency.Android/OpenTok.Net.webrtc.Dependency.Android.csproj

sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net8.0-android/" Bindings/OpenTok.Net.webrtc.Dependency.Android/OpenTok.Net.webrtc.Dependency.Android.csproj

dotnet pack Bindings/OpenTok.Net.webrtc.Dependency.Android/OpenTok.Net.webrtc.Dependency.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly

cd NugetPackages
rm -rf webrtc

unzip -n -q OpenTok.Net.webrtc.Dependency.Android.$1.7.nupkg -d webrtc
unzip -n -q OpenTok.Net.webrtc.Dependency.Android.$1.8.nupkg -d webrtc

rm OpenTok.Net.webrtc.Dependency.Android.$1.7.nupkg
rm OpenTok.Net.webrtc.Dependency.Android.$1.8.nupkg

# cp -R webrtc/Readme.md webrtc/Readme.txt


mkdir webrtc/native
mkdir webrtc/native/lib
mkdir webrtc/native/lib/android
cp -R webrtc/lib/net8.0-android34.0/webrtc.aar webrtc/native/lib/android

rm webrtc/lib/net8.0-android34.0/webrtc.aar
rm webrtc/lib/net7.0-android33.0/webrtc.aar 


sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$1.$2/" OpenTok.Net.webrtc.Dependency.Android.nuspec

nuget pack OpenTok.Net.webrtc.Dependency.Android.nuspec

rm -rf webrtc

if  [ -z "$3" ]
then
echo "package ready"
unzip -n -q OpenTok.Net.webrtc.Dependency.Android.$1.$2.nupkg -d webrtc
else 
dotnet nuget push OpenTok.Net.webrtc.Dependency.Android.$1.$2.nupkg --api-key $3 --source https://api.nuget.org/v3/index.json --timeout 3000000
# rm OpenTok.Net.webrtc.Dependency.Android.$1.$2.nupkg
fi
cd ..
fi