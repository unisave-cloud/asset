# copy the directory and go into the copy
cd ../
rm -rf ./AssetForExport
cp -r ./Asset ./AssetForExport
cd ./AssetForExport

# remove unnecessary files
rm ./Assets/Unisave/Resources/UnisavePreferencesFile.asset*
rm -r ./Assets/Unisave/Tests*

# remove acceptance tests
rm -r ./Assets/AcceptanceTests*

# remove example tests
rm -r ./Assets/Unisave/Examples/Tests*

