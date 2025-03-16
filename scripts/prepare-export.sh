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

# remove resources
rm -rf ./Assets/Resources*

# remove unisave fixture
rm -rf ./Assets/UnisaveFixture*

# remove TextMesh Pro
rm -rf ./Assets/TextMesh Pro*
