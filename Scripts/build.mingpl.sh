#!/bin/sh

# ./Scripts/build.mingpl.sh BUILD="1-beta1" VERSION="6.0.0" IOSVERSION="6.0.0" ANDROIDVERSION="6.0.0" MACVERSION="6.0.0"
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
echo "$IOSVERSION" > Bindings/FFmpegKit.Net.MinGpl.iOS/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.7/" Bindings/FFmpegKit.Net.MinGpl.iOS/FFmpegKit.Net.MinGpl.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net7.0-ios/" Bindings/FFmpegKit.Net.MinGpl.iOS/FFmpegKit.Net.MinGpl.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.MinGpl.iOS/FFmpegKit.Net.MinGpl.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.8/" Bindings/FFmpegKit.Net.MinGpl.iOS/FFmpegKit.Net.MinGpl.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net8.0-ios/" Bindings/FFmpegKit.Net.MinGpl.iOS/FFmpegKit.Net.MinGpl.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.MinGpl.iOS/FFmpegKit.Net.MinGpl.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf mingplios
unzip -n -q FFmpegKit.Net.MinGpl.iOS.$IOSVERSION.7.nupkg -d mingplios
unzip -n -q FFmpegKit.Net.MinGpl.iOS.$IOSVERSION.8.nupkg -d mingplios
rm FFmpegKit.Net.MinGpl.iOS.$IOSVERSION.7.nupkg
rm FFmpegKit.Net.MinGpl.iOS.$IOSVERSION.8.nupkg
cd ..
echo "ios part nugets generated"
echo "$MACVERSION" > Bindings/FFmpegKit.Net.MinGpl.Mac/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.7/" Bindings/FFmpegKit.Net.MinGpl.Mac/FFmpegKit.Net.MinGpl.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net7.0-maccatalyst/" Bindings/FFmpegKit.Net.MinGpl.Mac/FFmpegKit.Net.MinGpl.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.MinGpl.Mac/FFmpegKit.Net.MinGpl.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.8/" Bindings/FFmpegKit.Net.MinGpl.Mac/FFmpegKit.Net.MinGpl.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net8.0-maccatalyst/" Bindings/FFmpegKit.Net.MinGpl.Mac/FFmpegKit.Net.MinGpl.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.MinGpl.Mac/FFmpegKit.Net.MinGpl.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf mingplmac
unzip -n -q FFmpegKit.Net.MinGpl.Mac.$MACVERSION.7.nupkg -d mingplmac
unzip -n -q FFmpegKit.Net.MinGpl.Mac.$MACVERSION.8.nupkg -d mingplmac
rm FFmpegKit.Net.MinGpl.Mac.$MACVERSION.7.nupkg
rm FFmpegKit.Net.MinGpl.Mac.$MACVERSION.8.nupkg
cd ..
echo "mac part nugets generated"
echo "$ANDROIDVERSION" > Bindings/FFmpegKit.Net.MinGpl.Android/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.7/" Bindings/FFmpegKit.Net.MinGpl.Android/FFmpegKit.Net.MinGpl.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net7.0-android/" Bindings/FFmpegKit.Net.MinGpl.Android/FFmpegKit.Net.MinGpl.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.MinGpl.Android/FFmpegKit.Net.MinGpl.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.8/" Bindings/FFmpegKit.Net.MinGpl.Android/FFmpegKit.Net.MinGpl.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net8.0-android/" Bindings/FFmpegKit.Net.MinGpl.Android/FFmpegKit.Net.MinGpl.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.MinGpl.Android/FFmpegKit.Net.MinGpl.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf mingplandroid
unzip -n -q FFmpegKit.Net.MinGpl.Android.$ANDROIDVERSION.7.nupkg -d mingplandroid
unzip -n -q FFmpegKit.Net.MinGpl.Android.$ANDROIDVERSION.8.nupkg -d mingplandroid
rm FFmpegKit.Net.MinGpl.Android.$ANDROIDVERSION.7.nupkg
rm FFmpegKit.Net.MinGpl.Android.$ANDROIDVERSION.8.nupkg
cd ..
echo "android part nugets generated"
cd NugetPackages

cp -R mingplandroid/Readme.md mingplandroid/Readme.txt
cp -R mingplmac/Readme.md mingplmac/Readme.txt
cp -R mingplios/Readme.md mingplios/Readme.txt
cp -R mingpl/Readme.md mingpl/Readme.txt

# mkdir Voice/native
# mkdir Voice/native/lib
# mkdir Voice/native/lib/ios
# cp -R webrtc/lib/net8.0-android34.0/webrtc.aar webrtc/native/lib/android
# 
# rm webrtc/lib/net8.0-android34.0/webrtc.aar
# rm webrtc/lib/net7.0-android33.0/webrtc.aar 


sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$ANDROIDVERSION.$BUILD/" FFmpegKit.Net.MinGpl.Android.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$IOSVERSION.$BUILD/" FFmpegKit.Net.MinGpl.iOS.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$MACVERSION.$BUILD/" FFmpegKit.Net.MinGpl.Mac.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$VERSION.$BUILD/" FFmpegKit.Net.MinGpl.nuspec

nuget pack FFmpegKit.Net.MinGpl.Android.nuspec
nuget pack FFmpegKit.Net.MinGpl.iOS.nuspec
nuget pack FFmpegKit.Net.MinGpl.Mac.nuspec
nuget pack FFmpegKit.Net.MinGpl.nuspec

rm -rf mingplandroid
rm -rf mingplios
rm -rf mingplmac

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