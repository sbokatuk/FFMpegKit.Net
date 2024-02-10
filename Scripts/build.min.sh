#!/bin/sh

# ./Scripts/build.min.sh BUILD="1-beta1" VERSION="6.0.0" IOSVERSION="6.0.0" ANDROIDVERSION="6.0.0" MACVERSION="6.0.0"
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
echo "$IOSVERSION" > Bindings/FFmpegKit.Net.Min.iOS/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.7/" Bindings/FFmpegKit.Net.Min.iOS/FFmpegKit.Net.Min.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net7.0-ios/" Bindings/FFmpegKit.Net.Min.iOS/FFmpegKit.Net.Min.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.Min.iOS/FFmpegKit.Net.Min.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.8/" Bindings/FFmpegKit.Net.Min.iOS/FFmpegKit.Net.Min.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net8.0-ios/" Bindings/FFmpegKit.Net.Min.iOS/FFmpegKit.Net.Min.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.Min.iOS/FFmpegKit.Net.Min.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf minios
unzip -n -q FFmpegKit.Net.Min.iOS.$IOSVERSION.7.nupkg -d minios
unzip -n -q FFmpegKit.Net.Min.iOS.$IOSVERSION.8.nupkg -d minios
rm FFmpegKit.Net.Min.iOS.$IOSVERSION.7.nupkg
rm FFmpegKit.Net.Min.iOS.$IOSVERSION.8.nupkg
cd ..
echo "ios part nugets generated"
echo "$MACVERSION" > Bindings/FFmpegKit.Net.Min.Mac/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.7/" Bindings/FFmpegKit.Net.Min.Mac/FFmpegKit.Net.Min.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net7.0-maccatalyst/" Bindings/FFmpegKit.Net.Min.Mac/FFmpegKit.Net.Min.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.Min.Mac/FFmpegKit.Net.Min.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.8/" Bindings/FFmpegKit.Net.Min.Mac/FFmpegKit.Net.Min.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net8.0-maccatalyst/" Bindings/FFmpegKit.Net.Min.Mac/FFmpegKit.Net.Min.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.Min.Mac/FFmpegKit.Net.Min.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf minmac
unzip -n -q FFmpegKit.Net.Min.Mac.$MACVERSION.7.nupkg -d minmac
unzip -n -q FFmpegKit.Net.Min.Mac.$MACVERSION.8.nupkg -d minmac
rm FFmpegKit.Net.Min.Mac.$MACVERSION.7.nupkg
rm FFmpegKit.Net.Min.Mac.$MACVERSION.8.nupkg
cd ..
echo "mac part nugets generated"
echo "$ANDROIDVERSION" > Bindings/FFmpegKit.Net.Min.Android/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.7/" Bindings/FFmpegKit.Net.Min.Android/FFmpegKit.Net.Min.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net7.0-android/" Bindings/FFmpegKit.Net.Min.Android/FFmpegKit.Net.Min.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.Min.Android/FFmpegKit.Net.Min.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.8/" Bindings/FFmpegKit.Net.Min.Android/FFmpegKit.Net.Min.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net8.0-android/" Bindings/FFmpegKit.Net.Min.Android/FFmpegKit.Net.Min.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.Min.Android/FFmpegKit.Net.Min.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf minandroid
unzip -n -q FFmpegKit.Net.Min.Android.$ANDROIDVERSION.7.nupkg -d minandroid
unzip -n -q FFmpegKit.Net.Min.Android.$ANDROIDVERSION.8.nupkg -d minandroid
rm FFmpegKit.Net.Min.Android.$ANDROIDVERSION.7.nupkg
rm FFmpegKit.Net.Min.Android.$ANDROIDVERSION.8.nupkg
cd ..
echo "android part nugets generated"
cd NugetPackages

cp -R minandroid/Readme.md minandroid/Readme.txt
cp -R minmac/Readme.md minmac/Readme.txt
cp -R minios/Readme.md minios/Readme.txt
cp -R min/Readme.md min/Readme.txt

# mkdir Voice/native
# mkdir Voice/native/lib
# mkdir Voice/native/lib/ios
# cp -R webrtc/lib/net8.0-android34.0/webrtc.aar webrtc/native/lib/android
# 
# rm webrtc/lib/net8.0-android34.0/webrtc.aar
# rm webrtc/lib/net7.0-android33.0/webrtc.aar 


sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$ANDROIDVERSION.$BUILD/" FFmpegKit.Net.Min.Android.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$IOSVERSION.$BUILD/" FFmpegKit.Net.Min.iOS.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$MACVERSION.$BUILD/" FFmpegKit.Net.Min.Mac.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$VERSION.$BUILD/" FFmpegKit.Net.Min.nuspec

nuget pack FFmpegKit.Net.Min.Android.nuspec
nuget pack FFmpegKit.Net.Min.iOS.nuspec
nuget pack FFmpegKit.Net.Min.Mac.nuspec
nuget pack FFmpegKit.Net.Min.nuspec

rm -rf minandroid
rm -rf minios
rm -rf minmac

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