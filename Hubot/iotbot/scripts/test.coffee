request = require "request";
fs = require "fs";

cron = require('cron').CronJob;

module.exports = (robot) ->

  cron_job = null;
  signal_job = null;
  btc_monitor_job = null;

  robot.respond /vipstart (.*)|vipstart/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      path = "/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/twitterAPI/"
      command = "#{path}getTweetCron.pl"
      host = "https://slack.com/api/chat.postMessage"
      token = process.env.HUBOT_SLACK_TOKEN
      channel = msg.message.room
      cycle_sec = 60
      cycle_sec = arg if arg?
      stop = "#{path}DEST/cmdCode.json"
      command = "#{command} #{host} #{token} #{channel} #{cycle_sec} #{stop}"
      msg.send "exec getTweetCron()"
      @exec command, (error, stdout, stderr) ->
        #msg.send error if error?
        msg.send stdout if stdout?
        #msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /vipstop/i, (msg) ->
    if msg.message.user.name == "isaox"
      @exec = require('child_process').exec
      path = "/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/twitterAPI/"
      command = "#{path}cmdTweet.pl"
      command = "#{command} #{path} stop hoge hoge"
      msg.send "exec Stop-Command."
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /vipadd (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      args = msg.match[1].split(/\s/)
      @exec = require('child_process').exec
      path = "/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/twitterAPI/"
      command = "#{path}cmdTweet.pl"
      # room_name = robot.adapter.client.rtm.dataStore.getChannelGroupOrDMById( msg.message.room )
      user = args[0]
      room_id = robot.adapter.client.rtm.dataStore.getChannelOrGroupByName( args[1] ).id
      command = "#{command} #{path} add #{user} #{room_id}"
      msg.send "command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /vipdelete (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      path = "/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/twitterAPI/"
      command = "#{path}cmdTweet.pl"
      param = arg if arg?
      command = "#{command} #{path} delete #{param} hoge"
      msg.send "exec Delete-Command."
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /btcstart (.*)|btcstart/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      channel = msg.message.room
      @exec = require('child_process').exec
      path = "/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/bitflyerAPI/"
      command = "#{path}getPriceCron.pl"
      btc_host = "https://bitflyer.jp/api/echo/price"
      slack_host = "https://slack.com/api/files.upload"
      token = process.env.HUBOT_SLACK_TOKEN
      channel = msg.message.room
      list = "#{path}DEST/BtcPriceList.json"
      graph = "#{path}DEST/BtcPriceGraph.png"
      stop = "#{path}DEST/StopCode.txt"
      test = 0
      cycle_sec = 60
      sampling = 30
      threshold = 20000
      param = "#{sampling} #{threshold}"
      param = arg if arg?
      command = "#{command} #{btc_host} #{slack_host} #{token} #{channel} #{list} #{graph} #{stop} #{test} #{cycle_sec} #{param}"
      msg.send "exec getPriceCron() param=#{param} "
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /btcstop (.*)|btcstop/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      channel = msg.message.room
      @exec = require('child_process').exec
      path = "/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/bitflyerAPI/"
      command = "#{path}createStopCode.pl"
      stop = "#{path}DEST/StopCode.txt"
      stop = arg if arg?
      command = "#{command} #{stop}"
      msg.send "exec createStopCode() stop=#{stop} "
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

#  robot.respond /BTC_UP/i, (msg) ->
#    if msg.message.user.name == "isaox"
#      arg = msg.match[1]
#      channel = msg.message.room
#      path = "/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/bitflyerAPI/"
#      graph = "#{path}DEST/BtcPriceGraph.png"
#      fs.access("#{graph}", (error) =>
#        if(error)
#          msg.send "is-not exists"
#        else
#          msg.send "is exists"
#          api_url = "https://slack.com/api/"
#          channel = msg.message.room
#          options = {
#            token: process.env.HUBOT_SLACK_TOKEN,
#            filename: graph,
#            file: fs.createReadStream("#{graph}"),
#            channels: channel
#          }
#          request
#            .post {url:api_url + 'files.upload', formData: options}, (error, response, body) ->
#              if !error && response.statusCode == 200
#                msg.send "upload OK"
#                fs.unlink("#{graph}", (del_error) =>
#                  if(del_error)
#                    msg.send "can't delete Graph-File:#{graph}"
#                )
#              else
#                msg.send "NG status code: #{response.statusCode}"
#          )
#    else
#      msg.send "get out !!"

#  robot.respond /POSTSLACK (.*)|POSTSLACK/i, (msg) ->
#    if msg.message.user.name == "isaox"
#      arg = msg.match[1]
#      path = "/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/bitflyerAPI/"
#      command = "#{path}slackPost.pl"
#      host = "https://slack.com/api/chat.postMessage"
#      token = process.env.HUBOT_SLACK_TOKEN
#      channel = msg.message.room
#      text = ""
#      text = arg if arg?
#      command = "#{command} #{host} #{token} #{channel} #{text}"
#      #msg.send "token:#{token}"
#      #msg.send "channel:#{channel}"
#      #msg.send "text:#{text}"
#      @exec = require('child_process').exec
#      @exec command, (error, stdout, stderr) ->
#        msg.send error if error?
#        msg.send stdout if stdout?
#        msg.send stderr if stderr?
#    else
#      msg.send "get out !!"

#  robot.respond /uploadslack (.*)/i, (msg) ->
#    if msg.message.user.name == "isaox"
#      arg = msg.match[1]
#      path = "/home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/bitflyerAPI/"
#      command = "#{path}slackUpload.pl"
#      host = "https://slack.com/api/files.upload"
#      token = process.env.HUBOT_SLACK_TOKEN
#      channel = msg.message.room
#      filePath = ""
#      filePath = arg if arg?
#      command = "#{command} #{host} #{token} #{channel} #{filePath}"
#      #msg.send "token:#{token}"
#      #msg.send "channel:#{channel}"
#      #msg.send "text:#{filePath}"
#      @exec = require('child_process').exec
#      @exec command, (error, stdout, stderr) ->
#        msg.send error if error?
#        msg.send stdout if stdout?
#        msg.send stderr if stderr?
#    else
#      msg.send "get out !!"

#  robot.respond /testcron (.*)|testcron/i, (msg) ->
#    if msg.message.user.name == "isaox"
#      arg = msg.match[1]
#      msg.send "Arg[0]: #{arg}" if arg?
#      channel = msg.message.room
#      msg.send "respond from #{channel}."
#      # cron's 1st parameter
#      #   seconds      : 0-59
#      #   Minutes      : 0-59
#      #   Hours        : 0-23
#      #   Day of Month : 1-31
#      #   Months       : 0-11
#      #   Day of Week  : 0-6
#      if cron_job != null
#        msg.send "cron_job is exist."
#      else
#        msg.send "cron_job is-not exist."
#        cron_job = new cron '15 * * * * *', () =>
#          robot.send {room: channel}, "I send msg with regularity."
#        , null, true, "Asia/Tokyo"
#        msg.send "created cron_job."
#    else
#      msg.send "get out !!"

  robot.respond /timesignal (.*)|timesignal/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      msg.send "Arg[0]: #{arg}" if arg?
      channel = msg.message.room
      #msg.send "respond from #{channel}."
      if signal_job != null
        msg.send "signal_job is exist."
        if signal_job.running
          signal_job.stop()
          msg.send "signal_job is stop."
        else
          signal_job.start()
          msg.send "signal_job is start."
      else
        msg.send "signal_job is-not exist."
        signal_job = new cron '0 0 * * * *', () =>
          # get time.
          dt = new Date()
          year = dt.getFullYear()
          month = ("0"+( dt.getMonth() + 1 )).slice(-2)
          date = ("0"+dt.getDate()).slice(-2)
          hour = ("0"+dt.getHours()).slice(-2)
          min = ("0"+dt.getMinutes()).slice(-2)
          sec = ("0"+dt.getSeconds()).slice(-2)
          # time_msg = "いまは#{month}がつ#{date}にち#{hour}じ#{min}ふん#{sec}びょうです"
          time_msg = "#{hour}じ、#{min}ふん、になりました。"
          time_msg = "#{time_msg}#{arg}" if arg?
          robot.send {room: channel}, "#{time_msg}"
          #create command
          @exec = require('child_process').execSync
          command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/talkpi.sh #{time_msg}"
          @exec command, (error, stdout, stderr) ->
            msg.send error if error?
            msg.send stdout if stdout?
            msg.send stderr if stderr?
        , null, true, "Asia/Tokyo"
    else
      msg.send "get out !!"

  robot.respond /raspistill (.*)|raspistill/i, (msg) ->
    if msg.message.user.name == "isaox"
      # msg.send "match[0]: #{msg.match[0]}"
      # msg.send "match[1]: #{msg.match[1]}"
      arg = msg.match[1]
      dt = new Date()
      year = dt.getFullYear()
      month = ("0"+( dt.getMonth() + 1 )).slice(-2)
      date = ("0"+dt.getDate()).slice(-2)
      hour = ("0"+dt.getHours()).slice(-2)
      min = ("0"+dt.getMinutes()).slice(-2)
      sec = ("0"+dt.getSeconds()).slice(-2)
      file_path = "/home/pi/picam/"
      file_name = "#{year}-#{month}-#{date}_#{hour}#{min}_#{sec}.jpg"
      msg.send "file_name: #{file_name}"
      @execSync = require('child_process').execSync
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/stillpi.sh #{file_path}#{file_name} "
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @execSync command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
      api_url = "https://slack.com/api/"
      channel = msg.message.room
      options = {
        token: process.env.HUBOT_SLACK_TOKEN,
        filename: file_name,
        file: fs.createReadStream("#{file_path}#{file_name}"),
        channels: channel
      }

      request
        .post {url:api_url + 'files.upload', formData: options}, (error, response, body) ->
          if !error && response.statusCode == 200
            msg.send "OK"
          else
            msg.send "NG status code: #{response.statusCode}"
    else
      msg.send "get out !!"

  robot.hear /isaox/i, (res) ->
    res.send "isaox? isaox is my master!!"

  robot.hear /(くそがあああああ)|(くっそおおおおお)/i, (msg) ->
    kusoga_arry = ["https://pbs.twimg.com/media/C5k-X9aU4AAx2Pk.jpg:large","http://livedoor.blogimg.jp/guran2016_ms06/imgs/b/8/b8a2dc96-s.jpg"]
    kusoga_msg = msg.random kusoga_arry
    msg.send "#{kusoga_msg}"

  robot.hear /who am I/i, (msg) ->
    msg.send "You are #{msg.message.user.name}"

  robot.hear /who are You/i, (msg) ->
    msg.send "My name is iotbot! I am HUBOT!!"

  robot.respond /lsusb (.*)|lsusb/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/lsusb_cmd.sh"
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /ls (.*)|ls/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/ls_cmd.sh"
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /ps (.*)|ps/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/ps_cmd.sh"
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /dpkg (.*)|dpg/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/dpkg_cmd.sh"
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /ifconfig (.*)|ifconfig/i, (msg) ->
    if msg.message.user.name == "isaox"
      # msg.send "match[0]: #{msg.match[0]}"
      # msg.send "match[1]: #{msg.match[1]}"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/ifconfig_cmd.sh"
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /git (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/git_cmd.sh #{arg}"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /apt-get (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/apt-get_cmd.sh #{arg}"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /reboot/, (msg) ->
    if msg.message.user.name == "isaox"
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/cmd_reboot.sh"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /shutdown/, (msg) ->
    if msg.message.user.name == "isaox"
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/cmd_shutdown.sh"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /make (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      target = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/make_bin.sh #{target}"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /talkpi (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      arg = msg.match[1]
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/talkpi.sh #{arg}"
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /upload (.*)/i, (msg) ->
    if msg.message.user.name == "isaox"
      # msg.send "match[0]: #{msg.match[0]}"
      # msg.send "match[1]: #{msg.match[1]}"
      file_name = msg.match[1]
      msg.send "file_name: #{file_name}"
      api_url = "https://slack.com/api/"
      channel = msg.message.room
      options = {
        token: process.env.HUBOT_SLACK_TOKEN,
        filename: file_name,
        file: fs.createReadStream(file_name),
        channels: channel
      }

      request
        .post {url:api_url + 'files.upload', formData: options}, (error, response, body) ->
          if !error && response.statusCode == 200
            msg.send "OK"
          else
            msg.send "NG status code: #{response.statusCode}"

    else
      msg.send "get out !!"
