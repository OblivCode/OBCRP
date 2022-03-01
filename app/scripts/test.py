import urllib.request
import simplejson

id = 'JCMJI6PBmmo'
url = 'http://gdata.youtube.com/feeds/api/videos/%s?alt=json&v=2' % id

json = simplejson.load(urllib.request.urlopen(url))


title = json['entry']['title']['$t']
author = json['entry']['author'][0]['name']

print("id:%s\nauthor:%s\ntitle:%s".format(id, author, title))