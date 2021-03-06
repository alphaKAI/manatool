function build_scd(){
  dotnet publish -c Release -r win-x64
  dotnet publish -c Release -r osx-x64
  dotnet publish -c Release -r linux-x64
}

function download_and_copy_driver() {
  local win=https://chromedriver.storage.googleapis.com/2.45/chromedriver_win32.zip
  local linux=https://chromedriver.storage.googleapis.com/2.45/chromedriver_linux64.zip
  local mac=https://chromedriver.storage.googleapis.com/2.45/chromedriver_mac64.zip
  local old=`pwd`
  local base="bin/Release/netcoreapp2.2/"

  cd "$base/win-x64/"
  rm chromedriver chromedriver.exe
  if [ ! -e chromedriver_win32.zip ]; then
    wget $win
  fi
  unzip chromedriver_win32.zip
  cp chromedriver.exe publish
  cd $old

  cd "$base/linux-x64/"
  rm chromedriver
  if [ ! -e chromedriver_linux64.zip ]; then
    wget $linux
  fi
  unzip chromedriver_linux64.zip
  cp chromedriver publish
  cd $old

  cd "$base/osx-x64/"
  rm chromedriver
  if [ ! -e chromedriver_mac64.zip]; then
    wget $mac
  fi
  unzip chromedriver_mac64.zip
  cp chromedriver publish
  cd $old
}

function warpping() {
  local exec_name=manatool
  local out_name=manatool-single
  # Windows
  warp-packer --arch windows-x64 --input_dir bin/Release/netcoreapp2.2/win-x64/publish --exec $exec_name.exe --output $out_name.exe
  # Linux
  warp-packer --arch linux-x64 --input_dir bin/Release/netcoreapp2.2/linux-x64/publish --exec $exec_name --output $out_name.linux
  # macOS
  warp-packer --arch macos-x64 --input_dir bin/Release/netcoreapp2.2/osx-x64/publish --exec $exec_name --output $out_name.macos
}

function zipping() {
  zip manatool-win64.zip manatool-single.exe
  zip manatool-linux64.zip manatool-single.linux
  zip manatool-mac.zip manatool-single.macos
}

build_scd
download_and_copy_driver
warpping
zipping
