import json
from queue import Queue
from modules import discord
import threading

checked_services: list[str] = [] #services that are enabled
current_service: str = 'blah' #the current service displayed on RP
last_json: str = 'blah' #the last json recieved

def MainHandler(jsonQ: Queue, checkedQ: Queue):
    #start cu thread
    checked_updater(checkedQ, jsonQ)
    while True:
        global last_json
        last_json = jsonQ.get(True)
        web_bundle = json.loads(last_json)
        #print('checked: ' + repr(checked_services))
        
        def service_check(service: str) -> bool:
            enabled = service in checked_services
            if not enabled:
                return False
            
            if service == 'Twitch':
                url = 'twitch.tv'
            else:
                url = '{}.com'.format(service.lower().replace(' ', ''))
            if (url in web_bundle['url']):
                global current_service
                current_service = service
                
            return (url in web_bundle['url'])
        #YT 'YouTube' in checked_services and web_bundle['url'].startswith('https://www.youtube.com')
        if service_check('YouTube'):
            YTHandler(web_bundle)
        #GNow
        elif service_check('GeForce Now'):
            GNowHandler(web_bundle)
        #Twitch
        elif service_check('Twitch'):
            TwitchHandler(web_bundle)
        #SL
        elif service_check('SoloLearn'):
            SLHandler(web_bundle)
        

def checked_updater(cq: Queue, jq: Queue):
    def func(checkedQ: Queue, jsonQ: Queue):
        while True:
            checkedlist: list[str] = checkedQ.get(True)
            for x in checkedlist:
                if not x in checked_services:
                    checked_services.append(x)
            for x in checked_services:
                if not x in checkedlist:
                    checked_services.remove(x)
            if not current_service in checked_services:
                discord.ClearPresence()
            else:
                if not last_json == 'blah':
                    try:
                        jsonQ.get_nowait()
                    except:
                        pass
                    jsonQ.put_nowait(last_json)
        
    thread = threading.Thread(target=func, args=(cq,jq), name='Checked Updater Thread')
    thread.start()
    
#sub-handlers
def YTHandler(bundle: dict):
    state = 'YouTube'
    details = 'Loading'
    title: str = bundle['title']
    
    url_path: str = bundle['url'][bundle['url'].index('.com/') + 5:]
    
    if url_path == '':
        details = 'Browsing Home'
    elif url_path.startswith('feed'):
        page = url_path[5:].capitalize()
        if '?' in page:
            page = page[:page.index('?')]
        details = 'Browsing their ' + page
    elif url_path.startswith('playlist'):
        pl_name = title[:title.index(' - ')]
        details = 'Viewing {} playlist'.format(pl_name)
    elif url_path.startswith('watch'):
        vid_name = title[:title.index(' - ')]
        details = 'Watching ' + vid_name
    elif url_path.startswith('c'):
        try:
            ch_name = title[:title.index(' - ')]
            details = 'Viewing {}\'s channel'.format(ch_name)
        except:
            details = 'Checking out a channel'
    elif url_path.startswith('account'):
        details = 'Changing their settings'
    elif url_path.startswith('premium'):
        details = 'Considering Premium'
    else:
        details = 'Checking out ' + url_path.capitalize()
    
            
    discord.SetPresence(state, details, 'yt')

def GNowHandler(bundle: dict):
    state = 'GeForce NOW'
    details = 'Loading'
    title: str = bundle['title']
    url_path: str = bundle['url'][bundle['url'].index('.com/') + 5:]
     
    if title == 'GeForce NOW':
        details = 'Browsing games'
    elif 'on GeForce NOW' in title:
        idx = title.index('on GeForce NOW')
        name = title[0:idx]
        details = 'Playing ' + name
    
    discord.SetPresence(state, details, 'gnow')

def TwitchHandler(bundle: dict):
    state = 'Twitch'
    details = 'Loading'
    title: str = bundle['title']
    url_path: str = bundle['url'][bundle['url'].index('.tv') + 4:]
    if url_path == '':
        details = 'Browsing Main Page'
    elif url_path.startswith('/directory'):
        page = title[:title.index(' - ')].lower()
        details = 'Browsing ' + page
    elif title.lower().startswith(url_path):
        streamer = title[:title.index(' - ')]
        details = 'Watching ' + streamer
    else:
        def page():
            if not '/' in url_path:
                return url_path
            return url_path[url_path.rindex('/') + 1:]
        details = 'Viewing ' + page()
    details = details.replace('%20', ' ')
    discord.SetPresence(state, details, 'twitch')
     
   
SL_CodeNameDict = {
    'Web': '',
    'Python': 'py',
    'NodeJs': 'nodejs',
    'C': '1089',
    'C++': '1051|cpp',
    'C#': '1080',
    'Java': '1068',
    'Kotlin': '1160|kt',
    'R': '1147',
    'Go': '1164',
    'Swift': '1075',
    'Python Core': '1073',
    'Python for Beginners': '1157',
    'Intermediate Python': '1158',
    'Python for Data Science': '1161',
    'Python for Finance': '1139',
    'Python Data Structures': '1159',
    'Ruby': '1081|ruby',
    'PHP': '1059|php',
    'SQL': '1060',
    'jQuery': '1082',
    'JavaScript': '1024',
    'HTML': '1014',
    'CSS': '1023',
    'Responsive Web Design': '1162',
    'React + Redux': '1097',
    'Data Science': '1093',
    'Machine Learning': '1094',
    'Coding for Marketers': '1165',
    'Angular + NestJS': '1092',
    
}
def SLHandler(bundle: dict):
    state = 'YouTube'
    details = 'Loading'
    title: str = bundle['title']
    url_path: str = bundle['url'][bundle['url'].index('.com/') + 5:]
    
    if 'code.' in bundle['url']:
        id: str = bundle['url'][bundle['url'].rindex('/')+1:]
        if '#' in id:
            id = id[1:]
            for key in SL_CodeNameDict:
                val = SL_CodeNameDict[key]
                if key.lower().startswith(id):
                    id = key
                    break
                elif val.endswith(id):
                    id = key
                    break
        details = 'Coding ' + id.capitalize()
    elif url_path.startswith('profile'):
        details = 'Viewing profile'
    elif url_path.startswith('learning'):
        if url_path.startswith('learning/'):
            course_id = url_path[9:]
            for key in SL_CodeNameDict:
                val = SL_CodeNameDict[key]
                if val.startswith(course_id):
                    details = 'Learning ' + key
                    break
        else:
            details = 'Browsing learning resources'
    elif url_path.startswith('home'):
        details = 'Browsing Home'
    elif url_path.lower().startswith('discuss'):
        if url_path[0] == 'D' and len(url_path) >8:
            details = 'Viewing a discussion'
        else:
            details = 'Viewing discussions'
    else:
        details = 'Browsing ' + url_path
    if '?' in details:
        details = details[:details.index('?')].replace('/', '')
    discord.SetPresence(state, details, 'sl_logo')

def UnsupportedHandler():
    discord.ClearPresence()
