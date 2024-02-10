#!/bin/sh

# ./Scripts/build.fullgpl.sh BUILD="1-beta1" VERSION="6.0.0" IOSVERSION="6.0.0" ANDROIDVERSION="6.0.0" MACVERSION="6.0.0"
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
echo "$IOSVERSION" > Bindings/FFmpegKit.Net.FullGpl.iOS/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.7/" Bindings/FFmpegKit.Net.FullGpl.iOS/FFmpegKit.Net.FullGpl.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net7.0-ios/" Bindings/FFmpegKit.Net.FullGpl.iOS/FFmpegKit.Net.FullGpl.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.FullGpl.iOS/FFmpegKit.Net.FullGpl.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.8/" Bindings/FFmpegKit.Net.FullGpl.iOS/FFmpegKit.Net.FullGpl.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net8.0-ios/" Bindings/FFmpegKit.Net.FullGpl.iOS/FFmpegKit.Net.FullGpl.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.FullGpl.iOS/FFmpegKit.Net.FullGpl.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf fullgplios
unzip -n -q FFmpegKit.Net.FullGpl.iOS.$IOSVERSION.7.nupkg -d fullgplios
unzip -n -q FFmpegKit.Net.FullGpl.iOS.$IOSVERSION.8.nupkg -d fullgplios
rm FFmpegKit.Net.FullGpl.iOS.$IOSVERSION.7.nupkg
rm FFmpegKit.Net.FullGpl.iOS.$IOSVERSION.8.nupkg
cd ..
echo "ios part nugets generated"
echo "$MACVERSION" > Bindings/FFmpegKit.Net.FullGpl.Mac/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.7/" Bindings/FFmpegKit.Net.FullGpl.Mac/FFmpegKit.Net.FullGpl.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net7.0-maccatalyst/" Bindings/FFmpegKit.Net.FullGpl.Mac/FFmpegKit.Net.FullGpl.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.FullGpl.Mac/FFmpegKit.Net.FullGpl.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.8/" Bindings/FFmpegKit.Net.FullGpl.Mac/FFmpegKit.Net.FullGpl.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net8.0-maccatalyst/" Bindings/FFmpegKit.Net.FullGpl.Mac/FFmpegKit.Net.FullGpl.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.FullGpl.Mac/FFmpegKit.Net.FullGpl.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf fullgplmac
unzip -n -q FFmpegKit.Net.FullGpl.Mac.$MACVERSION.7.nupkg -d fullgplmac
unzip -n -q FFmpegKit.Net.FullGpl.Mac.$MACVERSION.8.nupkg -d fullgplmac
rm FFmpegKit.Net.FullGpl.Mac.$MACVERSION.7.nupkg
rm FFmpegKit.Net.FullGpl.Mac.$MACVERSION.8.nupkg
cd ..
echo "mac part nugets generated"
echo "$ANDROIDVERSION" > Bindings/FFmpegKit.Net.FullGpl.Android/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.7/" Bindings/FFmpegKit.Net.FullGpl.Android/FFmpegKit.Net.FullGpl.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net7.0-android/" Bindings/FFmpegKit.Net.FullGpl.Android/FFmpegKit.Net.FullGpl.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.FullGpl.Android/FFmpegKit.Net.FullGpl.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.8/" Bindings/FFmpegKit.Net.FullGpl.Android/FFmpegKit.Net.FullGpl.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net8.0-android/" Bindings/FFmpegKit.Net.FullGpl.Android/FFmpegKit.Net.FullGpl.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.FullGpl.Android/FFmpegKit.Net.FullGpl.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf fullgplandroid
unzip -n -q FFmpegKit.Net.FullGpl.Android.$ANDROIDVERSION.7.nupkg -d fullgplandroid
unzip -n -q FFmpegKit.Net.FullGpl.Android.$ANDROIDVERSION.8.nupkg -d fullgplandroid
rm FFmpegKit.Net.FullGpl.Android.$ANDROIDVERSION.7.nupkg
rm FFmpegKit.Net.FullGpl.Android.$ANDROIDVERSION.8.nupkg
cd ..
echo "android part nugets generated"
cd NugetPackages

cp -R fullgplandroid/Readme.md fullgplandroid/Readme.txt
cp -R fullgplmac/Readme.md fullgplmac/Readme.txt
cp -R fullgplios/Readme.md fullgplios/Readme.txt
cp -R fullgpl/Readme.md fullgpl/Readme.txt

# mkdir Voice/native
# mkdir Voice/native/lib
# mkdir Voice/native/lib/ios
# cp -R webrtc/lib/net8.0-android34.0/webrtc.aar webrtc/native/lib/android
# 
# rm webrtc/lib/net8.0-android34.0/webrtc.aar
# rm webrtc/lib/net7.0-android33.0/webrtc.aar 


sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$ANDROIDVERSION.$BUILD/" FFmpegKit.Net.FullGpl.Android.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$IOSVERSION.$BUILD/" FFmpegKit.Net.FullGpl.iOS.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$MACVERSION.$BUILD/" FFmpegKit.Net.FullGpl.Mac.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$VERSION.$BUILD/" FFmpegKit.Net.FullGpl.nuspec

nuget pack FFmpegKit.Net.FullGpl.Android.nuspec
nuget pack FFmpegKit.Net.FullGpl.iOS.nuspec
nuget pack FFmpegKit.Net.FullGpl.Mac.nuspec
nuget pack FFmpegKit.Net.FullGpl.nuspec

rm -rf fullgplandroid
rm -rf fullgplios
rm -rf fullgplmac

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