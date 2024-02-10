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
# ./Scripts/create.sh GITHUB="https://github.com/sbokatuk/Net.Agora" NMSC="Net.Agora.Video" VENDOR="Agora" DDD="Video" SITE="https://www.agora.io/en/" PLATFORM="Mac" smth="video"

rm -rf Bindings/$NMSC.$PLATFORM
# cp -R Scripts/nuspecs/Net.nuspec NugetPackages/$NMSC.nuspec
# sed -E -i "" "s=NMSC=$NMSC=" NugetPackages/$NMSC.nuspec
# sed -E -i "" "s=GITHUB=$GITHUB=" NugetPackages/$NMSC.nuspec
# sed -E -i "" "s=VENDOR=$VENDOR=" NugetPackages/$NMSC.nuspec
# sed -E -i "" "s=DDD=$DDD=" NugetPackages/$NMSC.nuspec
# sed -E -i "" "s=VENDOR=$VENDOR=" NugetPackages/$NMSC.nuspec
# sed -E -i "" "s=DDD=$DDD=" NugetPackages/$NMSC.nuspec
# sed -E -i "" "s=SITE=$SITE=" NugetPackages/$NMSC.nuspec
# sed -E -i "" "s=smth=$smth=" NugetPackages/$NMSC.nuspec

cp -R Scripts/nuspecs/$PLATFORM.nuspec NugetPackages/$NMSC.$PLATFORM.nuspec
./Scripts/replace.sh GITHUB="$GITHUB" NMSC="$NMSC" VENDOR="$VENDOR" DDD="$DDD" SITE="$SITE" smth="$smth" INFILE="NugetPackages/$NMSC.$PLATFORM.nuspec"

mkdir Bindings/$NMSC.$PLATFORM
cp -R Scripts/pgj/NMSC.$PLATFORM/* Bindings/$NMSC.$PLATFORM/

mv Bindings/$NMSC.$PLATFORM/NMSC.$PLATFORM.csproj Bindings/$NMSC.$PLATFORM/$NMSC.$PLATFORM.csproj
./Scripts/replace.sh GITHUB="$GITHUB" NMSC="$NMSC" VENDOR="$VENDOR" DDD="$DDD" SITE="$SITE" smth="$smth" INFILE="Bindings/$NMSC.$PLATFORM/$NMSC.$PLATFORM.csproj"
mv Bindings/$NMSC.$PLATFORM/NMSC.$PLATFORM.targets Bindings/$NMSC.$PLATFORM/$NMSC.$PLATFORM.targets
./Scripts/replace.sh GITHUB="$GITHUB" NMSC="$NMSC" VENDOR="$VENDOR" DDD="$DDD" SITE="$SITE" smth="$smth" INFILE="Bindings/$NMSC.$PLATFORM/$NMSC.$PLATFORM.targets"
./Scripts/replace.sh GITHUB="$GITHUB" NMSC="$NMSC" VENDOR="$VENDOR" DDD="$DDD" SITE="$SITE" smth="$smth" INFILE="Bindings/$NMSC.$PLATFORM/README.md"

if [[ "$PLATFORM" == "Mac" ]]
then
./Scripts/replace.sh GITHUB="$GITHUB" NMSC="$NMSC" VENDOR="$VENDOR" DDD="$DDD" SITE="$SITE" smth="$smth" INFILE="Bindings/$NMSC.$PLATFORM/ApiDefinition.cs"
./Scripts/replace.sh GITHUB="$GITHUB" NMSC="$NMSC" VENDOR="$VENDOR" DDD="$DDD" SITE="$SITE" smth="$smth" INFILE="Bindings/$NMSC.$PLATFORM/StructsAndEnums.cs"
fi

if [[ "$PLATFORM" == "iOS" ]]
then
./Scripts/replace.sh GITHUB="$GITHUB" NMSC="$NMSC" VENDOR="$VENDOR" DDD="$DDD" SITE="$SITE" smth="$smth" INFILE="Bindings/$NMSC.$PLATFORM/ApiDefinition.cs"
./Scripts/replace.sh GITHUB="$GITHUB" NMSC="$NMSC" VENDOR="$VENDOR" DDD="$DDD" SITE="$SITE" smth="$smth" INFILE="Bindings/$NMSC.$PLATFORM/StructsAndEnums.cs"
fi

# if [[ "$PLATFORM" == "Android" ]]
# then
# fi

# sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$IOSVERSION.7/" Bindings/Agora.Net.Voice.iOS/Agora.Net.Voice.iOS.csproj