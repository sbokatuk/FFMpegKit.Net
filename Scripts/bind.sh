#!/bin/sh
rm -rf Bindings/OpenTok.Net.iOS/lib/cs
mkdir Bindings/OpenTok.Net.iOS/lib/cs

cp -R Bindings/OpenTok.Net.iOS/lib/OpenTok.xcframework/ios-arm64/OpenTok.framework Bindings/OpenTok.Net.iOS/lib/cs
cd Bindings/OpenTok.Net.iOS/lib/cs

sharpie bind -scope OpenTok -output=. -framework ./OpenTok.framework
cd ../../../..
mv Bindings/OpenTok.Net.iOS/lib/cs/ApiDefinitions.cs Bindings/OpenTok.Net.iOS/ApiDefinitions.new.cs
mv Bindings/OpenTok.Net.iOS/lib/cs/StructsAndEnums.cs Bindings/OpenTok.Net.iOS/StructsAndEnums.new.cs
rm -rf Bindings/OpenTok.Net.iOS/lib/cs
