if [ -z "$1" ]
then
echo "No version Argument for Opentok"
else
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$1/" Bindings/OpenTok.Net.iOS/OpenTok.Net.iOS.csproj
sed -E -i "" "s/<Version>([0-9]{1,}\.)+[0-9]{1,}/<Version>$1/" Bindings/OpenTok.Net.Android/OpenTok.Net.Android.csproj
sed -E -i "" "s/<version>([0-9]{1,}\.)+[0-9]{1,}/<version>$1/" NugetPackages/OpenTok.Net.nuspec
echo "Version for Opentok set to $1"
fi