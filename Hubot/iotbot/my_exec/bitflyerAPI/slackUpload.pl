#!/usr/bin/perl

use strict;
#use Furl;
#use HTTP::Request::Common;

my $host = shift;
my $token = shift;
my $channel = shift;
my $filePath = shift;

#print "host=$host\n";
#print "token=$token\n";
#print "channel_id=$channel_id\n";
#print "filepath=$filePath\n";

my $curlCmd = "curl -o /dev/null -s -F file=\@$filePath -F channels=$channel -F token=$token $host";
system($curlCmd);

#my $req = POST ($host,
#    'Content' => [
#        token => $token,
#        channels => $channel_id,
#        filename => $filePath,
#        file     => $filePath,
#        filetype => 'javascript'
#    ]);
#my $res = Furl->new->request($req);
#my $res_code = $res->code;
#my $res_msg = $res->message;
#print "response-code:$res_code\n";
#print "response-msg:$res_msg\n";

exit 0;
