#!/bin/sh

# ./Scripts/build.full.sh BUILD="1-beta1" VERSION="6.0.0" IOSVERSION="6.0.0" ANDROIDVERSION="6.0.0" MACVERSION="6.0.0"
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
echo "$IOSVERSION" > Bindings/FFmpegKit.Net.Full.iOS/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.7/" Bindings/FFmpegKit.Net.Full.iOS/FFmpegKit.Net.Full.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net7.0-ios/" Bindings/FFmpegKit.Net.Full.iOS/FFmpegKit.Net.Full.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.Full.iOS/FFmpegKit.Net.Full.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.8/" Bindings/FFmpegKit.Net.Full.iOS/FFmpegKit.Net.Full.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net8.0-ios/" Bindings/FFmpegKit.Net.Full.iOS/FFmpegKit.Net.Full.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.Full.iOS/FFmpegKit.Net.Full.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf fullios
unzip -n -q FFmpegKit.Net.Full.iOS.$IOSVERSION.7.nupkg -d fullios
unzip -n -q FFmpegKit.Net.Full.iOS.$IOSVERSION.8.nupkg -d fullios
rm FFmpegKit.Net.Full.iOS.$IOSVERSION.7.nupkg
rm FFmpegKit.Net.Full.iOS.$IOSVERSION.8.nupkg
cd ..
echo "ios part nugets generated"
echo "$MACVERSION" > Bindings/FFmpegKit.Net.Full.Mac/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.7/" Bindings/FFmpegKit.Net.Full.Mac/FFmpegKit.Net.Full.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net7.0-maccatalyst/" Bindings/FFmpegKit.Net.Full.Mac/FFmpegKit.Net.Full.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.Full.Mac/FFmpegKit.Net.Full.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.8/" Bindings/FFmpegKit.Net.Full.Mac/FFmpegKit.Net.Full.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net8.0-maccatalyst/" Bindings/FFmpegKit.Net.Full.Mac/FFmpegKit.Net.Full.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.Full.Mac/FFmpegKit.Net.Full.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf fullmac
unzip -n -q FFmpegKit.Net.Full.Mac.$MACVERSION.7.nupkg -d fullmac
unzip -n -q FFmpegKit.Net.Full.Mac.$MACVERSION.8.nupkg -d fullmac
rm FFmpegKit.Net.Full.Mac.$MACVERSION.7.nupkg
rm FFmpegKit.Net.Full.Mac.$MACVERSION.8.nupkg
cd ..
echo "mac part nugets generated"
echo "$ANDROIDVERSION" > Bindings/FFmpegKit.Net.Full.Android/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.7/" Bindings/FFmpegKit.Net.Full.Android/FFmpegKit.Net.Full.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net7.0-android/" Bindings/FFmpegKit.Net.Full.Android/FFmpegKit.Net.Full.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.Full.Android/FFmpegKit.Net.Full.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.8/" Bindings/FFmpegKit.Net.Full.Android/FFmpegKit.Net.Full.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net8.0-android/" Bindings/FFmpegKit.Net.Full.Android/FFmpegKit.Net.Full.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.Full.Android/FFmpegKit.Net.Full.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf fullandroid
unzip -n -q FFmpegKit.Net.Full.Android.$ANDROIDVERSION.7.nupkg -d fullandroid
unzip -n -q FFmpegKit.Net.Full.Android.$ANDROIDVERSION.8.nupkg -d fullandroid
rm FFmpegKit.Net.Full.Android.$ANDROIDVERSION.7.nupkg
rm FFmpegKit.Net.Full.Android.$ANDROIDVERSION.8.nupkg
cd ..
echo "android part nugets generated"
cd NugetPackages

cp -R fullandroid/Readme.md fullandroid/Readme.txt
cp -R fullmac/Readme.md fullmac/Readme.txt
cp -R fullios/Readme.md fullios/Readme.txt
cp -R full/Readme.md full/Readme.txt

# mkdir Voice/native
# mkdir Voice/native/lib
# mkdir Voice/native/lib/ios
# cp -R webrtc/lib/net8.0-android34.0/webrtc.aar webrtc/native/lib/android
# 
# rm webrtc/lib/net8.0-android34.0/webrtc.aar
# rm webrtc/lib/net7.0-android33.0/webrtc.aar 


sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$ANDROIDVERSION.$BUILD/" FFmpegKit.Net.Full.Android.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$IOSVERSION.$BUILD/" FFmpegKit.Net.Full.iOS.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$MACVERSION.$BUILD/" FFmpegKit.Net.Full.Mac.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$VERSION.$BUILD/" FFmpegKit.Net.Full.nuspec

nuget pack FFmpegKit.Net.Full.Android.nuspec
nuget pack FFmpegKit.Net.Full.iOS.nuspec
nuget pack FFmpegKit.Net.Full.Mac.nuspec
nuget pack FFmpegKit.Net.Full.nuspec

rm -rf fullandroid
rm -rf fullios
rm -rf fullmac

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