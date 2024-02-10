#!/bin/sh

./cleanup.sh lib
mkdir Bindings/OpenTok.Net.Android/lib
mkdir Bindings/OpenTok.Net.iOS/lib
mkdir Downloads


#ios
cd Downloads
git clone https://github.com/opentok/opentok-ios-sdk-samples

cd opentok-ios-sdk-samples/Basic-Video-Chat-XCFramework

pod install

pod update

cd ../../..

mv Downloads/opentok-ios-sdk-samples/Basic-Video-Chat-XCFramework/Pods/OTXCFramework/OpenTok.xcframework Bindings/OpenTok.Net.iOS/lib

iOSVersion=$(cat Downloads/opentok-ios-sdk-samples/Basic-Video-Chat-XCFramework/Podfile.lock | grep "OTXCFramework (=" | grep -Eo '([0-9]{1,}\.)+[0-9]{1,}');

sed -E -i "" "s/<ReleaseVersion>([0-9]{1,}\.)+[0-9]{1,}/<ReleaseVersion>${iOSVersion}.7/" Bindings/OpenTok.Net.iOS/OpenTok.Net.iOS.csproj


echo "iOS lib downloaded!"
#android

curl -L https://repo1.maven.org/maven2/com/opentok/android/opentok-android-sdk/maven-metadata.xml > Downloads/android.xml
AndroidVersion=$(cat Downloads/android.xml | grep "<latest>" | grep -Eo '([0-9]{1,}\.)+[0-9]{1,}');
# curl -L https://repo1.maven.org/maven2/com/opentok/android/opentok-android-sdk/$AndroidVersion/opentok-android-sdk-$AndroidVersion.aar > Downloads/opentok-android-sdk.aar
# ignore previous row - download from parsed pom file
curl -L https://repo1.maven.org/maven2/com/opentok/android/opentok-android-sdk/$AndroidVersion/opentok-android-sdk-$AndroidVersion.pom > Downloads/opentok-android-sdk.pom

sed -E -i "" "s/<ReleaseVersion>([0-9]{1,}\.)+[0-9]{1,}/<ReleaseVersion>${AndroidVersion}.1/" Bindings/OpenTok.Net.Android/OpenTok.Net.Android.csproj

groups=$(grep groupId Downloads/opentok-android-sdk.pom | sed -E 's/<.{0,1}groupId>//g' | awk '{print $1}');
artifacts=$(grep artifactId Downloads/opentok-android-sdk.pom | sed -E 's/<.{0,1}artifactId>//g' | awk '{print $1}');
versions=$(grep version Downloads/opentok-android-sdk.pom | sed -E 's/<.{0,1}version>//g' | tail -n +2 | awk '{print $1}');

count=$(grep groupId Downloads/opentok-android-sdk.pom | sed -E 's/<.{0,1}groupId>//g' | awk '{print $1}' | wc -l | awk '{print $1}')

for i in $(seq "$count")
do
currentGroup=$(echo "$groups" | sed -n ''$i' p');
groupLink=${currentGroup//./\/}
currentArtifact=$(echo "$artifacts" | sed -n ''$i' p');
currentVersion=$(echo "$versions" | sed -n ''$i' p');

if [[ "$currentVersion" != "<?xml" ]]
then
if [[ $currentGroup =~ ^"com.google" ]] || [[ $currentGroup =~ ^"androidx" ]]
then
echo "Skip download $i of $count android lib, looks like default (google or android): $groupLink/$currentArtifact/$currentVersion"
else
echo "Downloading $i of $count android libs: https://repo1.maven.org/maven2/$groupLink/$currentArtifact/$currentVersion/$currentArtifact-$currentVersion.aar"
curl -L https://repo1.maven.org/maven2/$groupLink/$currentArtifact/$currentVersion/$currentArtifact-$currentVersion.aar > Downloads/$currentArtifact.aar
fi
fi
done

mv Downloads/*.aar Bindings/OpenTok.Net.Android/lib

rm -rf Downloads
echo "Downloaded Opentok framework with version for ios $iOSVersion and android $AndroidVersion"
echo "Verify that your android binding project referenced all dependency libs, they already downloaded into same folder Android/lib"


#sharpie bind -scope OpenTok -output=CS -framework ./OpenTok.framework 
