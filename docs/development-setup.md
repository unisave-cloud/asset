# Development Setup (📦 asset)


## Setting up

- Clone the repo `git clone https://github.com/unisave-cloud/asset`
- Follow the [After Cloning](../README.md#after-cloning) checklist in the root README file.


## New feature development

- Add the `-dev` suffix to the version in `AssetMeta.cs`.
- Add the feature and commit changes.


## Deploying new version to GitHub (15 - 30 min)

- Update the `Documentation.pdf` file in `Assets/Plugins/Unisave`.
- Eemove the `-dev` suffix and commit the new version to github (or make a `-rc.1` release candidate)
- Close the Unity Editor
- Create *export-copy* of the folder
    - `bash scripts/prepare-export.sh`
- Open `asset-for-export` in Unity Editor
- Go to the `Assets/Plugins` folder, right-click the `Unisave` folder and choose `Export package...`
- Select all files, export into downloads folder and name it `unisave-asset-1.2.3-alpha.unitypackage`
- Create a github release page and attach the `.unitypackage` there


## Deploying further to the asset store

- Install *Asset Store Publishing Tools* in the `asset-for-export` project (from "my assets")
- Open menu `Asset Store Tools > Asset Store Uploader` and log in
- Open the asset in the publisher portal (https://publisher.unity.com/) and click the blue button `Create new draft to edit`
- Eefresh the upload tools and select the draft
- Select "upload from pre-exported .unitypackage file" and select the file that was uploaded to github
- Upload the package
- Ignore the popup stating you need newer version of unity (yes for new uploads, not for updates)
- Fill out and check all the tabs of the package draft
- Click submit
