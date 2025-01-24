echo "Freeing up disk space"

echo "Listing packages"
dpkg-query -Wf '${Installed-Size}\t${Package}\n' | sort -n | tail -n 100
df -h
echo "Removing large packages"
sudo apt-get remove -y '^ghc-8.*'
sudo apt-get remove -y '^dotnet-.*'
sudo apt-get remove -y '^llvm-.*'
sudo apt-get remove -y 'php.*'
sudo apt-get remove -y azure-cli hhvm google-chrome-stable firefox powershell mono-devel
sudo apt-get autoremove -y
sudo apt-get clean
df -h
echo "Removing large directories"
rm -rf /usr/share/dotnet
rm -rf /usr/local/lib/android
rm -rf /opt/ghc
df -h