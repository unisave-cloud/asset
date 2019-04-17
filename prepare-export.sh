# copy the directory and go into the copy
cd ../
rm -rf ./UnisaveForExport
cp -r ./Unisave ./UnisaveForExport
cd ./UnisaveForExport

# remove unnecessary files
rm ./Assets/Unisave/Resources/UnisavePreferencesFile.asset*
rm -r ./Assets/Unisave/Editor/Tests*

