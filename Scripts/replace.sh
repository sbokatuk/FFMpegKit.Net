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
# INFILE
# ./Scripts/replace.sh GITHUB="https://github.com/sbokatuk/Net.Agora" NMSC="Net.Agora.Video" VENDOR="Agora" DDD="Video" SITE="https://www.agora.io/en/" smth="video"


sed -E -i "" "s=NMSC=$NMSC=" $INFILE
sed -E -i "" "s=GITHUB=$GITHUB=" $INFILE
sed -E -i "" "s=VENDOR=$VENDOR=" $INFILE
sed -E -i "" "s=DDD=$DDD=" $INFILE
sed -E -i "" "s=VENDOR=$VENDOR=" $INFILE
sed -E -i "" "s=DDD=$DDD=" $INFILE
sed -E -i "" "s=SITE=$SITE=" $INFILE
sed -E -i "" "s=smth=$smth=" $INFILE
