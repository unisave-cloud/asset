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
rm ./Assets/Plugins/Unisave/Resources/UnisavePreferencesFile.asset*

# remove unisave fixture
rm -r ./Assets/UnisaveFixture*

# disable Heapstore backend
HEAPSTORE_BACKEND_FILE=./Assets/Plugins/Unisave/Heapstore/Backend/HeapstoreBackend.asset
sed -i "s/uploadBehaviour: always/uploadBehaviour: never/" $HEAPSTORE_BACKEND_FILE
