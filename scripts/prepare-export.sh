# Prepare Export
# --------------
#
# This script prepares the repository for export to Unity Asset Store.
# It does that by making a copy of this directory and preparing it.

# copy the asset directory to asset-for-export and go into the copy
cd ../
rm -rf ./asset-for-export
cp -r ./asset ./asset-for-export
cd ./asset-for-export

# remove unnecessary unisave files
rm ./Assets/Unisave/Resources/UnisavePreferencesFile.asset*

# remove unisave fixture
rm -r ./Assets/UnisaveFixture*

