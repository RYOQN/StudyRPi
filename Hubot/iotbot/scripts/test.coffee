request = require "request";
fs = require "fs";
# dt = require "Date";

module.exports = (robot) ->

  robot.respond /still (.*)|still/i, (msg) ->
    if msg.message.user.name == "isaox"
      # msg.send "match[0]: #{msg.match[0]}"
      # msg.send "match[1]: #{msg.match[1]}"
      arg = msg.match[1]
      # year = dt.getFullYear()
      # month = dt.getMonth() + 1
      # date = dt.getDate()
      # hour = dt.getHours()
      # min = dt.getMinutes()
      # file_name = "#{year}-#{month}-#{date}_#{hour}#{min}.jpg"
      ile_name = "#{arg}.jpg"
      msg.send "file_name: #{file_name}"
      # @exec = require('child_process').exec
      # command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/stillpi.sh"
      # command = "#{command} #{arg}" if arg?
      # msg.send "Command: #{command}"
      # @exec command, (error, stdout, stderr) ->
      #   msg.send error if error?
      #   msg.send stdout if stdout?
      #   msg.send stderr if stderr?

      # api_url = "https://slack.com/api/"
      # channel = msg.message.room
      # options = {
      #   token: process.env.HUBOT_SLACK_TOKEN,
      #   filename: file_name,
      #   file: fs.createReadStream('/home/pi/picam/' + file_name),
      #   channels: channel
      # }

      # request
      #   .post {url:api_url + 'files.upload', formData: options}, (error, response, body) ->
      #     if !error && response.statusCode == 200
      #       msg.send "OK"
      #     else
      #       msg.send "NG status code: #{response.statusCode}"
    else
      msg.send "get out !!"

  robot.hear /isaox/i, (res) ->
    res.send "isaox? isaox is my master!!"

  robot.hear /who am I/i, (msg) ->
    msg.send "You are #{msg.message.user.name}"

  robot.hear /who are You/i, (msg) ->
    msg.send "My name is iotbot! I am HUBOT!!"

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

  robot.respond /raspistill (.*)|raspistill/i, (msg) ->
    if msg.message.user.name == "isaox"
      # msg.send "match[0]: #{msg.match[0]}"
      # msg.send "match[1]: #{msg.match[1]}"
      arg = msg.match[1]
      # msg.send "arg: #{arg}"
      # msg.send "arg is defined" if arg?
      # msg.send "arg is undefined" unless arg?
      @exec = require('child_process').exec
      command = "sudo -u pi sh /home/pi/GitHub/StudyRPi/Hubot/iotbot/my_exec/stillpi.sh"
      command = "#{command} #{arg}" if arg?
      msg.send "Command: #{command}"
      @exec command, (error, stdout, stderr) ->
        msg.send error if error?
        msg.send stdout if stdout?
        msg.send stderr if stderr?
    else
      msg.send "get out !!"

  robot.respond /stillup (.*)/i, (msg) ->
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
        file: fs.createReadStream('/home/pi/picam/' + file_name),
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