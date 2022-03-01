
# OBCRP
A discord rich presence utility for websites<br/>
[![pypresence](https://img.shields.io/badge/using-pypresence-00bb88.svg?style=for-the-badge&logo=discord&logoWidth=20)](https://github.com/qwertyquerty/pypresence)
<br/>
Currently supported browsers:<br/>
Opera, Opera GX
(this does not necessarily mean other browsers won't work)
<br/><br/>
# How to launch <br/>
1. All you have to do is run 'launch.sh'
<br/>
# How to install extension <br/>
1. Drag crx file onto your browser's extension page.<br/>
2. Wait for the extension to install.<br/>
(Enable developer mode if it does not work)<br/>
<br/>
# How to use
1. Make sure extension is enabled and the app is open.<br/>
2. Simply, check the boxes for which ever website you want rich presence to be enabled for.<br/>
<br/>
# Adding custom services
- Add a handler<br/>
In services.py, add your own handler which accepts a dictionary as a parameter. The dictionary will contain two keys: 'title' and 'url', which belongs to the user's current open tab.<br/>
In function: MainHandler, at the end, put the custom service's name through service check then if true, call your handler.<br/>
In app.py, add the custom service's name to services_list.<br/>
If the custom service doesn't use .com then you'll have to replace the url in the service check function. See how twich url is replaced as help.