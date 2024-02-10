#!/bin/sh

# ./Scripts/build.video.sh BUILD="1-beta1" VERSION="6.0.0" IOSVERSION="6.0.0" ANDROIDVERSION="6.0.0" MACVERSION="6.0.0"
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
echo "$IOSVERSION" > Bindings/FFmpegKit.Net.Video.iOS/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.7/" Bindings/FFmpegKit.Net.Video.iOS/FFmpegKit.Net.Video.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net7.0-ios/" Bindings/FFmpegKit.Net.Video.iOS/FFmpegKit.Net.Video.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.Video.iOS/FFmpegKit.Net.Video.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.8/" Bindings/FFmpegKit.Net.Video.iOS/FFmpegKit.Net.Video.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net8.0-ios/" Bindings/FFmpegKit.Net.Video.iOS/FFmpegKit.Net.Video.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.Video.iOS/FFmpegKit.Net.Video.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf videoios
unzip -n -q FFmpegKit.Net.Video.iOS.$IOSVERSION.7.nupkg -d videoios
unzip -n -q FFmpegKit.Net.Video.iOS.$IOSVERSION.8.nupkg -d videoios
rm FFmpegKit.Net.Video.iOS.$IOSVERSION.7.nupkg
rm FFmpegKit.Net.Video.iOS.$IOSVERSION.8.nupkg
cd ..
echo "ios part nugets generated"
echo "$MACVERSION" > Bindings/FFmpegKit.Net.Video.Mac/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.7/" Bindings/FFmpegKit.Net.Video.Mac/FFmpegKit.Net.Video.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net7.0-maccatalyst/" Bindings/FFmpegKit.Net.Video.Mac/FFmpegKit.Net.Video.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.Video.Mac/FFmpegKit.Net.Video.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.8/" Bindings/FFmpegKit.Net.Video.Mac/FFmpegKit.Net.Video.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net8.0-maccatalyst/" Bindings/FFmpegKit.Net.Video.Mac/FFmpegKit.Net.Video.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.Video.Mac/FFmpegKit.Net.Video.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf videomac
unzip -n -q FFmpegKit.Net.Video.Mac.$MACVERSION.7.nupkg -d videomac
unzip -n -q FFmpegKit.Net.Video.Mac.$MACVERSION.8.nupkg -d videomac
rm FFmpegKit.Net.Video.Mac.$MACVERSION.7.nupkg
rm FFmpegKit.Net.Video.Mac.$MACVERSION.8.nupkg
cd ..
echo "mac part nugets generated"
echo "$ANDROIDVERSION" > Bindings/FFmpegKit.Net.Video.Android/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.7/" Bindings/FFmpegKit.Net.Video.Android/FFmpegKit.Net.Video.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net7.0-android/" Bindings/FFmpegKit.Net.Video.Android/FFmpegKit.Net.Video.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.Video.Android/FFmpegKit.Net.Video.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.8/" Bindings/FFmpegKit.Net.Video.Android/FFmpegKit.Net.Video.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net8.0-android/" Bindings/FFmpegKit.Net.Video.Android/FFmpegKit.Net.Video.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.Video.Android/FFmpegKit.Net.Video.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf videoandroid
unzip -n -q FFmpegKit.Net.Video.Android.$ANDROIDVERSION.7.nupkg -d videoandroid
unzip -n -q FFmpegKit.Net.Video.Android.$ANDROIDVERSION.8.nupkg -d videoandroid
rm FFmpegKit.Net.Video.Android.$ANDROIDVERSION.7.nupkg
rm FFmpegKit.Net.Video.Android.$ANDROIDVERSION.8.nupkg
cd ..
echo "android part nugets generated"
cd NugetPackages

cp -R videoandroid/Readme.md videoandroid/Readme.txt
cp -R videomac/Readme.md videomac/Readme.txt
cp -R videoios/Readme.md videoios/Readme.txt
cp -R video/Readme.md video/Readme.txt

# mkdir Voice/native
# mkdir Voice/native/lib
# mkdir Voice/native/lib/ios
# cp -R webrtc/lib/net8.0-android34.0/webrtc.aar webrtc/native/lib/android
# 
# rm webrtc/lib/net8.0-android34.0/webrtc.aar
# rm webrtc/lib/net7.0-android33.0/webrtc.aar 


sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$ANDROIDVERSION.$BUILD/" FFmpegKit.Net.Video.Android.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$IOSVERSION.$BUILD/" FFmpegKit.Net.Video.iOS.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$MACVERSION.$BUILD/" FFmpegKit.Net.Video.Mac.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$VERSION.$BUILD/" FFmpegKit.Net.Video.nuspec

nuget pack FFmpegKit.Net.Video.Android.nuspec
nuget pack FFmpegKit.Net.Video.iOS.nuspec
nuget pack FFmpegKit.Net.Video.Mac.nuspec
nuget pack FFmpegKit.Net.Video.nuspec

rm -rf videoandroid
rm -rf videoios
rm -rf videomac

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