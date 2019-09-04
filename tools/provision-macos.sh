#!/bin/bash -e
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

MONO_SELECT_VERSION=6.4.0
INSTALLER_URIS=(
  https://download.visualstudio.microsoft.com/download/pr/df65956a-5d32-4cc6-a05c-f4e42f6727b2/a2d824bd4d9d276e7ee4067eca4035ce/monoframework-mdk-6.4.0.165.macos10.xamarin.universal.pkg
  https://download.visualstudio.microsoft.com/download/pr/300000f9-c488-42be-a244-9cb0b95769d8/3b0bb55bd8b027ebd1c3baaf43f8b800/xamarin.mac-5.16.1.9.pkg
)

function status {
  local green
  local reset
  if command -v tput &>/dev/null; then
    green=$(tput setaf 2 2>/dev/null || exit 0)
    reset=$(tput sgr0 2>/dev/null || exit 0)
  fi
  echo "${green}$*${reset}"
}

pushd "$(dirname "$0")" &>/dev/null

mkdir -p _artifacts
pushd _artifacts &>/dev/null
for installer_uri in "${INSTALLER_URIS[@]}"; do
  installer_file=$(basename "${installer_uri}")
  if [ ! -f "${installer_file}" ]; then
    status "Downloading ${installer_file}..."
    curl -LO "${installer_uri}"
  fi

  status "Installing ${installer_file}..."
  sudo installer -pkg "${installer_file}" -target / -verboseR
done
popd &>/dev/null

status "Selecting Mono ${MONO_SELECT_VERSION}..."
sudo ./select-mono.sh "${MONO_SELECT_VERSION}"

status Done.