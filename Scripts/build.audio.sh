#!/bin/sh

# ./Scripts/build.audio.sh BUILD="1-beta1" VERSION="6.0.0" IOSVERSION="6.0.0" ANDROIDVERSION="6.0.0" MACVERSION="6.0.0"
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
echo "$IOSVERSION" > Bindings/FFmpegKit.Net.Audio.iOS/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.7/" Bindings/FFmpegKit.Net.Audio.iOS/FFmpegKit.Net.Audio.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net7.0-ios/" Bindings/FFmpegKit.Net.Audio.iOS/FFmpegKit.Net.Audio.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.Audio.iOS/FFmpegKit.Net.Audio.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.8/" Bindings/FFmpegKit.Net.Audio.iOS/FFmpegKit.Net.Audio.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net8.0-ios/" Bindings/FFmpegKit.Net.Audio.iOS/FFmpegKit.Net.Audio.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.Audio.iOS/FFmpegKit.Net.Audio.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf audioios
unzip -n -q FFmpegKit.Net.Audio.iOS.$IOSVERSION.7.nupkg -d audioios
unzip -n -q FFmpegKit.Net.Audio.iOS.$IOSVERSION.8.nupkg -d audioios
rm FFmpegKit.Net.Audio.iOS.$IOSVERSION.7.nupkg
rm FFmpegKit.Net.Audio.iOS.$IOSVERSION.8.nupkg
cd ..
echo "ios part nugets generated"
echo "$MACVERSION" > Bindings/FFmpegKit.Net.Audio.Mac/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.7/" Bindings/FFmpegKit.Net.Audio.Mac/FFmpegKit.Net.Audio.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net7.0-maccatalyst/" Bindings/FFmpegKit.Net.Audio.Mac/FFmpegKit.Net.Audio.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.Audio.Mac/FFmpegKit.Net.Audio.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.8/" Bindings/FFmpegKit.Net.Audio.Mac/FFmpegKit.Net.Audio.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net8.0-maccatalyst/" Bindings/FFmpegKit.Net.Audio.Mac/FFmpegKit.Net.Audio.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.Audio.Mac/FFmpegKit.Net.Audio.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf audiomac
unzip -n -q FFmpegKit.Net.Audio.Mac.$MACVERSION.7.nupkg -d audiomac
unzip -n -q FFmpegKit.Net.Audio.Mac.$MACVERSION.8.nupkg -d audiomac
rm FFmpegKit.Net.Audio.Mac.$MACVERSION.7.nupkg
rm FFmpegKit.Net.Audio.Mac.$MACVERSION.8.nupkg
cd ..
echo "mac part nugets generated"
echo "$ANDROIDVERSION" > Bindings/FFmpegKit.Net.Audio.Android/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.7/" Bindings/FFmpegKit.Net.Audio.Android/FFmpegKit.Net.Audio.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net7.0-android/" Bindings/FFmpegKit.Net.Audio.Android/FFmpegKit.Net.Audio.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.Audio.Android/FFmpegKit.Net.Audio.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.8/" Bindings/FFmpegKit.Net.Audio.Android/FFmpegKit.Net.Audio.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net8.0-android/" Bindings/FFmpegKit.Net.Audio.Android/FFmpegKit.Net.Audio.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.Audio.Android/FFmpegKit.Net.Audio.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf audioandroid
unzip -n -q FFmpegKit.Net.Audio.Android.$ANDROIDVERSION.7.nupkg -d audioandroid
unzip -n -q FFmpegKit.Net.Audio.Android.$ANDROIDVERSION.8.nupkg -d audioandroid
rm FFmpegKit.Net.Audio.Android.$ANDROIDVERSION.7.nupkg
rm FFmpegKit.Net.Audio.Android.$ANDROIDVERSION.8.nupkg
cd ..
echo "android part nugets generated"
cd NugetPackages

cp -R audioandroid/Readme.md audioandroid/Readme.txt
cp -R audiomac/Readme.md audiomac/Readme.txt
cp -R audioios/Readme.md audioios/Readme.txt
cp -R audio/Readme.md audio/Readme.txt

# mkdir Voice/native
# mkdir Voice/native/lib
# mkdir Voice/native/lib/ios
# cp -R webrtc/lib/net8.0-android34.0/webrtc.aar webrtc/native/lib/android
# 
# rm webrtc/lib/net8.0-android34.0/webrtc.aar
# rm webrtc/lib/net7.0-android33.0/webrtc.aar 


sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$ANDROIDVERSION.$BUILD/" FFmpegKit.Net.Audio.Android.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$IOSVERSION.$BUILD/" FFmpegKit.Net.Audio.iOS.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$MACVERSION.$BUILD/" FFmpegKit.Net.Audio.Mac.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$VERSION.$BUILD/" FFmpegKit.Net.Audio.nuspec

nuget pack FFmpegKit.Net.Audio.Android.nuspec
nuget pack FFmpegKit.Net.Audio.iOS.nuspec
nuget pack FFmpegKit.Net.Audio.Mac.nuspec
nuget pack FFmpegKit.Net.Audio.nuspec

rm -rf audioandroid
rm -rf audioios
rm -rf audiomac

# if  [ -z "$3" ]
# then
# echo "package ready"
# unzip -n -q OpenTok.Net.webrtc.Dependency.Android.$VERSION.$2.nupkg -d webrtc
# else 
# dotnet nuget push OpenTok.Net.webrtc.Dependency.Android.$VERSION.$2.nupkg --api-key $3 --source https://api.nuget.org/v3/index.json --timeout 3000000
# # rm OpenTok.Net.webrtc.Dependency.Android.$VERSION.$2.nupkg
# fi
# cd ..
fi