# copy the directory and go into the copy
cd ../
rm -rf ./AssetForExport
cp -r ./Asset ./AssetForExport
cd ./AssetForExport

# remove unnecessary files
rm ./Assets/Unisave/Resources/UnisavePreferencesFile.asset*
rm -r ./Assets/Unisave/Editor/Tests*

# remove chess example (not finished yet)
rm -r ./Assets/Unisave/Examples/Chess*
