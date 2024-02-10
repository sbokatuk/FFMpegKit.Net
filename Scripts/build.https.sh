#!/bin/sh

# ./Scripts/build.https.sh BUILD="1-beta1" VERSION="6.0.0" IOSVERSION="6.0.0" ANDROIDVERSION="6.0.0" MACVERSION="6.0.0"
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
echo "$IOSVERSION" > Bindings/FFmpegKit.Net.Https.iOS/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.7/" Bindings/FFmpegKit.Net.Https.iOS/FFmpegKit.Net.Https.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net7.0-ios/" Bindings/FFmpegKit.Net.Https.iOS/FFmpegKit.Net.Https.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.Https.iOS/FFmpegKit.Net.Https.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.8/" Bindings/FFmpegKit.Net.Https.iOS/FFmpegKit.Net.Https.iOS.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-ios/<TargetFramework>net8.0-ios/" Bindings/FFmpegKit.Net.Https.iOS/FFmpegKit.Net.Https.iOS.csproj
dotnet pack Bindings/FFmpegKit.Net.Https.iOS/FFmpegKit.Net.Https.iOS.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf httpsios
unzip -n -q FFmpegKit.Net.Https.iOS.$IOSVERSION.7.nupkg -d httpsios
unzip -n -q FFmpegKit.Net.Https.iOS.$IOSVERSION.8.nupkg -d httpsios
rm FFmpegKit.Net.Https.iOS.$IOSVERSION.7.nupkg
rm FFmpegKit.Net.Https.iOS.$IOSVERSION.8.nupkg
cd ..
echo "ios part nugets generated"
echo "$MACVERSION" > Bindings/FFmpegKit.Net.Https.Mac/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.7/" Bindings/FFmpegKit.Net.Https.Mac/FFmpegKit.Net.Https.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net7.0-maccatalyst/" Bindings/FFmpegKit.Net.Https.Mac/FFmpegKit.Net.Https.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.Https.Mac/FFmpegKit.Net.Https.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$MACVERSION.8/" Bindings/FFmpegKit.Net.Https.Mac/FFmpegKit.Net.Https.Mac.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-maccatalyst/<TargetFramework>net8.0-maccatalyst/" Bindings/FFmpegKit.Net.Https.Mac/FFmpegKit.Net.Https.Mac.csproj
dotnet pack Bindings/FFmpegKit.Net.Https.Mac/FFmpegKit.Net.Https.Mac.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf httpsmac
unzip -n -q FFmpegKit.Net.Https.Mac.$MACVERSION.7.nupkg -d httpsmac
unzip -n -q FFmpegKit.Net.Https.Mac.$MACVERSION.8.nupkg -d httpsmac
rm FFmpegKit.Net.Https.Mac.$MACVERSION.7.nupkg
rm FFmpegKit.Net.Https.Mac.$MACVERSION.8.nupkg
cd ..
echo "mac part nugets generated"
echo "$ANDROIDVERSION" > Bindings/FFmpegKit.Net.Https.Android/Readme.md
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.7/" Bindings/FFmpegKit.Net.Https.Android/FFmpegKit.Net.Https.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net7.0-android/" Bindings/FFmpegKit.Net.Https.Android/FFmpegKit.Net.Https.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.Https.Android/FFmpegKit.Net.Https.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$ANDROIDVERSION.8/" Bindings/FFmpegKit.Net.Https.Android/FFmpegKit.Net.Https.Android.csproj
sed -E -i "" "s/<TargetFramework>net([0-9]{1,}\.)+[0-9]{1,}-android/<TargetFramework>net8.0-android/" Bindings/FFmpegKit.Net.Https.Android/FFmpegKit.Net.Https.Android.csproj
dotnet pack Bindings/FFmpegKit.Net.Https.Android/FFmpegKit.Net.Https.Android.csproj --output NugetPackages --force --verbosity quiet --property WarningLevel=0 /clp:ErrorsOnly
cd NugetPackages
rm -rf httpsandroid
unzip -n -q FFmpegKit.Net.Https.Android.$ANDROIDVERSION.7.nupkg -d httpsandroid
unzip -n -q FFmpegKit.Net.Https.Android.$ANDROIDVERSION.8.nupkg -d httpsandroid
rm FFmpegKit.Net.Https.Android.$ANDROIDVERSION.7.nupkg
rm FFmpegKit.Net.Https.Android.$ANDROIDVERSION.8.nupkg
cd ..
echo "android part nugets generated"
cd NugetPackages

cp -R httpsandroid/Readme.md httpsandroid/Readme.txt
cp -R httpsmac/Readme.md httpsmac/Readme.txt
cp -R httpsios/Readme.md httpsios/Readme.txt
cp -R https/Readme.md https/Readme.txt

# mkdir Voice/native
# mkdir Voice/native/lib
# mkdir Voice/native/lib/ios
# cp -R webrtc/lib/net8.0-android34.0/webrtc.aar webrtc/native/lib/android
# 
# rm webrtc/lib/net8.0-android34.0/webrtc.aar
# rm webrtc/lib/net7.0-android33.0/webrtc.aar 


sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$ANDROIDVERSION.$BUILD/" FFmpegKit.Net.Https.Android.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$IOSVERSION.$BUILD/" FFmpegKit.Net.Https.iOS.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$MACVERSION.$BUILD/" FFmpegKit.Net.Https.Mac.nuspec
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$VERSION.$BUILD/" FFmpegKit.Net.Https.nuspec

nuget pack FFmpegKit.Net.Https.Android.nuspec
nuget pack FFmpegKit.Net.Https.iOS.nuspec
nuget pack FFmpegKit.Net.Https.Mac.nuspec
nuget pack FFmpegKit.Net.Https.nuspec

rm -rf httpsandroid
rm -rf httpsios
rm -rf httpsmac

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