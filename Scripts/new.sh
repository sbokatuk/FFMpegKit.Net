#!/bin/sh

for ARGUMENT in "$@"
do
   KEY=$(echo $ARGUMENT | cut -f1 -d=)

   KEY_LENGTH=${#KEY}
   VALUE="${ARGUMENT:$KEY_LENGTH+1}"

   export "$KEY"="$VALUE"
done

# GITHUB
# NMSC
# VENDOR
# DDD
# SITE
# smth
# PLATFORM

# ./Scripts/new.sh GITHUB="https://github.com/sbokatuk/Net.Agora" NMSC="Net.Agora.Video" VENDOR="Agora" DDD="Video" SITE="https://www.agora.io/en/" PLATFORM="iOS" smth="video" VERSION="4.2.6" IOSVERSION="4.2.6" ANDROIDVERSION="4.2.6" BUILD="1" MACVERSION="4.2.6" 
rm -rf NugetPackages/$smth
mkdir NugetPackages/$smth

cp -R Scripts/nuspecs/smth/* NugetPackages/$smth

cp -R Scripts/nuspecs/Net.nuspec NugetPackages/$NMSC.nuspec
sed -E -i "" "s=NMSC=$NMSC=" NugetPackages/$NMSC.nuspec
sed -E -i "" "s=GITHUB=$GITHUB=" NugetPackages/$NMSC.nuspec
sed -E -i "" "s=VENDOR=$VENDOR=" NugetPackages/$NMSC.nuspec
sed -E -i "" "s=DDD=$DDD=" NugetPackages/$NMSC.nuspec
sed -E -i "" "s=VENDOR=$VENDOR=" NugetPackages/$NMSC.nuspec
sed -E -i "" "s=DDD=$DDD=" NugetPackages/$NMSC.nuspec
sed -E -i "" "s=SITE=$SITE=" NugetPackages/$NMSC.nuspec
sed -E -i "" "s=smth=$smth=" NugetPackages/$NMSC.nuspec


./Scripts/create.sh GITHUB="$GITHUB" NMSC="$NMSC" VENDOR="$VENDOR" DDD="$DDD" SITE="$SITE" smth="$smth" PLATFORM="Mac"
./Scripts/create.sh GITHUB="$GITHUB" NMSC="$NMSC" VENDOR="$VENDOR" DDD="$DDD" SITE="$SITE" smth="$smth" PLATFORM="iOS"
./Scripts/create.sh GITHUB="$GITHUB" NMSC="$NMSC" VENDOR="$VENDOR" DDD="$DDD" SITE="$SITE" smth="$smth" PLATFORM="Android"
cp -R Scripts/build.smth.sh Scripts/build.$smth.sh
sed -E -i "" "s=NMSC=$NMSC=" Scripts/build.$smth.sh
sed -E -i "" "s=smth=$smth=" Scripts/build.$smth.sh
sed -E -i "" "s=NMSC=$NMSC=" Scripts/build.$smth.sh
sed -E -i "" "s=smth=$smth=" Scripts/build.$smth.sh
sed -E -i "" "s=NMSC=$NMSC=" Scripts/build.$smth.sh
sed -E -i "" "s=smth=$smth=" Scripts/build.$smth.sh

./Scripts/build.version.sh VERSION="$VERSION" IOSVERSION="$IOSVERSION" ANDROIDVERSION="$ANDROIDVERSION" BUILD="1-beta1" MACVERSION="$MACVERSION" NMSC="$NMSC" VENDOR="$VENDOR" smth="$smth"
sed -E -i "" "s=1-beta1=1=" NugetPackages/$NMSC.nuspec

./Scripts/publish.sh $KEY

