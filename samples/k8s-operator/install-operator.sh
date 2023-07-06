#!/bin/bash
file=operator-installer-linux
if [ ! -f $file ]; then
   echo "Downloading $file"
    wget "https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/raw/master/deploy/operator/installer/releases/$file"
fi
chmod +x $file
./$file