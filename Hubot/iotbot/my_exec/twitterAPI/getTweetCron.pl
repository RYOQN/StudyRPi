#!/usr/bin/perl

use strict;
use warnings;

use JSON;

use utf8;
use Encode;

use Time::Local;
use Time::Piece;

use Fcntl;

use Encode 'decode';
use Encode 'encode';

use Net::SSL;
use Net::Twitter::Lite::WithAPIv1_1;
use Data::Dumper;

use Date::Manip;

use Mozilla::CA;
$ENV{HTTPS_CA_FILE} = Mozilla::CA::SSL_ca_file();

use Furl;
use HTTP::Request::Common;

use DateTime::Format::HTTP;

my $host = shift;
my $token = shift;
my $channel = shift;
my $cycle_sec = shift;
my $cmdCode = shift;

my $env_path="./my_exec/twitterAPI";
#my $env_path=".";

#my $authSlack;
#if(readJson(\$authSlack, "$env_path/AuthSlack.json")!=0){
#    print "FileReadError. authTwitter.\n";
#    exit -1;
#}
#my $host = $authSlack->{"host"};
#my $token = $authSlack->{"token"};
#my $channel = $authSlack->{"channel"};
#my $cycle_sec = 60;
#my $stopCode = "$env_path/DEST/StopCode.txt";

my $authTwitter;
if(readJson(\$authTwitter, "$env_path/AuthTwitter.json")!=0){
    print "FileReadError. Auth.\n";
    exit -1;
}

my $nt = Net::Twitter::Lite::WithAPIv1_1->new(
    consumer_key    =>  $authTwitter->{"consumer_key"},
    consumer_secret =>  $authTwitter->{"consumer_secret"},
    access_token    =>  $authTwitter->{"access_token"},
    access_token_secret => $authTwitter->{"access_token_secret"},
    ssl =>  1,
);

while(1){
    eval{
        my $vipBCH;
        if(readJson(\$vipBCH, "$env_path/vipBCH.json")!=0){
            print "FileReadError. vipBCH.\n";
            exit -1;
        }

        my @keys_BCH = keys %{$vipBCH};

        for(my $i=0; $i<@keys_BCH; $i++){
            #print "keys_BCH[$i]:$keys_BCH[$i]\n";
            my $username = $keys_BCH[$i];
            

            my $res_timeline =  $nt->user_timeline(
                                    {
                                        screen_name => $username,
                                        count       => 1,
                                        since_id    => $vipBCH->{$username}->{"since_id"},
                                        include_rts => $vipBCH->{$username}->{"include_rts"},   # 0:RTが含まれない
                                        exclude_replies => "false",                                  # 0:リプライを含む
                                        trim_user => "true",                                        # 0:ユーザ情報を含む
                                        include_entities => "false",                                # 0:entities情報が含まれない
                                        contributor_details => "false",                             # 0:貢献者のscreen_nameが含まれない
                                    }
                                );
            my $timeline_num = @{$res_timeline};
            #print "timeline_num=$timeline_num\n";
            my $next_since_id = 0;
            for(my $j=0; $j<@{$res_timeline}; $j++){
                my $tweet_ref = \%{ $res_timeline->[$j] };

                # 返信先取得
                my $reply_to = "";
                if( exists $tweet_ref->{"in_reply_to_screen_name"} ){
                    $reply_to = $tweet_ref->{"in_reply_to_screen_name"};
                    if(!defined $reply_to){
                        $reply_to = "";
                    }
                }

                # Link取得
                my $id = $tweet_ref->{"id"};
                if($next_since_id < $id ){
                    $next_since_id = $id;
                }
                my $tweet_link = "https://twitter.com/$username/status/$id" . "\n";
                #print $tweet_link;

                # 日付取得
                my $tweet_date = "";
                if( exists $tweet_ref->{"created_at"} ){
                    my $created_at = $tweet_ref->{"created_at"};
                    # 終了時間をGMTからJSTに変換して現時間との差を計算する。
                    my $jst_date="";
                    convertTimeTZtoJST(\$jst_date, $created_at);
                    # convertTimeGMTtoJST(\$jst, $created_at);
                    $tweet_date = "*" . "$jst_date" ."*" . "\n";
                }

                # 本文取得
                my $tweet_text = "";
                if( exists $tweet_ref->{"text"} ){
                    my $text = $tweet_ref->{"text"};
                    # print $text . "\n";
                    $tweet_text = $text ."\n";
                }

                # 添付ファイルURL取得
                # print "exists extended_entities\n";
                my $extended_text = "";
                if( exists $tweet_ref->{"extended_entities"} ){
                    # print "exists media\n";
                    my $extended = $tweet_ref->{"extended_entities"};
                    if( exists $extended->{"media"} ){
                        my $media = $extended->{"media"};
                        for(my $k=0; $k<@{$media}; $k++){
                            my $mediaInfo = \%{ $media->[$k] };
                            if( exists $mediaInfo->{"media_url_https"}){
                                my $media_url = $mediaInfo->{"media_url_https"};
                                $extended_text = $extended_text . $media_url . "\n";
                            }
                        }
                    }
                }

                # 引用元取得
                my $quoted_text = "";
                if( exists $tweet_ref->{"quoted_status"} ){
                    my $quoted = $tweet_ref->{"quoted_status"};
                    if( exists $quoted->{"text"} ){
                        if( exists $quoted->{"created_at"} ){
                            my $quoted_date = $quoted->{"created_at"};
                            my $quoted_date_jst="";
                            convertTimeTZtoJST(\$quoted_date_jst, $quoted_date);
                            $quoted_text =  $quoted_text . "> " . $quoted_date_jst . "\n";
                            my $text = $quoted->{"text"};
                            my @strArray = split(/\n/, $text);
                            for(my $k=0; $k<@strArray; $k++){
                                $quoted_text =  $quoted_text . "> ". $strArray[$k] . "\n";
                            }
                        }
                    }
                }

                # RT元取得
                my $rt_text = "";
                my $rt_date = "";
                my $rt_quoted = "";
                my $rt_extended = "";
                if( exists $tweet_ref->{"retweeted_status"} ){
                    my $retweeted = $tweet_ref->{"retweeted_status"};
                    if( exists $retweeted->{"text"} ){
                        if( exists $retweeted->{"created_at"} ){
                            my $retweeted_date = $retweeted->{"created_at"};
                            my $retweeted_date_jst="";
                            convertTimeTZtoJST(\$retweeted_date_jst, $retweeted_date);
                            $rt_date =  "*" . $retweeted_date_jst . "*" . "\n";
                            my $retweeted_text = $retweeted->{"text"};
                            $rt_text =  $rt_text . $retweeted_text . "\n";

                            # RT引用元取得
                            if( exists $retweeted->{"quoted_status"} ){
                                my $quoted = $retweeted->{"quoted_status"};
                                if( exists $quoted->{"text"} ){
                                    if( exists $quoted->{"created_at"} ){
                                        my $quoted_date = $quoted->{"created_at"};
                                        my $quoted_date_jst="";
                                        convertTimeTZtoJST(\$quoted_date_jst, $quoted_date);
                                        my $quoted_text = $quoted->{"text"};
                                        $rt_quoted =  $rt_quoted . "> " . $quoted_date_jst . "\n";

                                        my @strArray = split(/\n/, $quoted_text);
                                        for(my $k=0; $k<@strArray; $k++){
                                            $rt_quoted =  $rt_quoted . "> ". $strArray[$k] . "\n";
                                        }
                                    }
                                }
                            }

                            # 添付ファイルURL取得
                            if( exists $retweeted->{"extended_entities"} ){
                                my $extended = $retweeted->{"extended_entities"};
                                if( exists $extended->{"media"} ){
                                    my $media = $extended->{"media"};
                                    for(my $k=0; $k<@{$media}; $k++){
                                        my $mediaInfo = \%{ $media->[$k] };
                                        if( exists $mediaInfo->{"media_url_https"}){
                                            my $media_url = $mediaInfo->{"media_url_https"};
                                            $rt_extended = $rt_extended . $media_url . "\n";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
             
                if( ($reply_to eq "") || ($reply_to eq $username) ){

                    my $post_text = "";
                    if($rt_quoted eq ""){
                        $post_text = $tweet_date . "```\n" . $tweet_link . "\n" . $tweet_text . "```\n" . $quoted_text . $extended_text;
                        my @array = ();
                        getHttpStrArray(\@array, $tweet_text, 0);
                        for(my $k=0;$k<@array;$k++){
                            my $beg_pos = index($array[$k], "https://t.co/");
                            if($beg_pos!=-1){
                                next;
                            }
                            $post_text = $post_text . $array[$k] . "\n";
                            #print $array[$k] . "\n";
                        }
                    }else{
                        $post_text = $rt_date . "```\n" . $tweet_link . "\n" . $rt_text . "```\n" . $rt_quoted . $rt_extended;
                        my @array = ();
                        getHttpStrArray(\@array, $rt_text, 0);
                        for(my $k=0;$k<@array;$k++){
                            my $beg_pos = index($array[$k], "https://t.co/");
                            if($beg_pos!=-1){
                                next;
                            }
                            $post_text = $post_text . $array[$k] . "\n";
                            #print $array[$k] . "\n";
                        }
                    }

                    my $postname = $username;
                    if( exists $vipBCH->{$username}->{"name"} ){
                        my $name = $vipBCH->{$username}->{"name"};
                        $postname = $name . " @" . $username;
                    }

                    my $send_channel = $channel;
                    if( exists $vipBCH->{$username}->{"channel_id"} ){
                        $send_channel = $vipBCH->{$username}->{"channel_id"};
                        # print "send_channel: $send_channel \n";
                    }
                    
                    my $req = POST ($host,
                        'Content' => [
                            token => $token,
                            channel => $send_channel,
                            username => $postname,
                            icon_url => $vipBCH->{$username}->{"profile_image_url_https"},
                            text => $post_text
                        ]);
                    my $res = Furl->new->request($req);
                }else{
                    #print "reply_to is not mine. $reply_to \n";
                }
                
                my $tweetFileName = sprintf("%s/DEST/%s_tweet.json", $env_path, $username);
                if(writeJson(\$tweet_ref, $tweetFileName, ">")!=0){
                    print "FileWriteError. " . $tweetFileName ."\n";
                    next;
                }

                sleep(3);
            }
            if($next_since_id>0){
                #print "since_id=$next_since_id\n";
                $vipBCH->{$username}->{"since_id"} = $next_since_id;
                if(writeJson(\$vipBCH, "$env_path/vipBCH.json", ">")!=0){
                    print "FileWriteError. vipBCH.\n";
                    next;
                }
            }
        }
    };

    if(-e $cmdCode){
        print "recieved cmdCode:$cmdCode\n";

        my $cmd_ref;
        if(readJson(\$cmd_ref, $cmdCode)!=0){
            print "FileReadError. $cmdCode \n";
            next;
        }

        my $vipBCH;
        if(readJson(\$vipBCH, "$env_path/vipBCH.json")!=0){
            print "FileReadError. vipBCH.\n";
            next;
        }

        my $isStop = 0;
        if(exists $cmd_ref->{"add"}){
            my $screen_name = $cmd_ref->{"add"}->{"screen_name"};
            my $imgUrl      = $cmd_ref->{"add"}->{"profile_image_url_https"};
            my $since_id    = $cmd_ref->{"add"}->{"since_id"};
            my $name        = $cmd_ref->{"add"}->{"name"};

            $vipBCH->{$screen_name}->{"include_rts"}="true";
            $vipBCH->{$screen_name}->{"profile_image_url_https"}=$imgUrl;
            $vipBCH->{$screen_name}->{"since_id"}=$since_id;
            $vipBCH->{$screen_name}->{"name"}=$name;

            if(exists $cmd_ref->{"add"}->{"channel_id"}){
                my $channel_id = $cmd_ref->{"add"}->{"channel_id"};
                $vipBCH->{$screen_name}->{"channel_id"}=$channel_id;
            }

            if(writeJson(\$vipBCH, "$env_path/vipBCH.json", ">")!=0){
                print "FileWriteError. vipBCH.\n";
                next;
            }
        }elsif(exists $cmd_ref->{"delete"}){
            my $screen_name = $cmd_ref->{"delete"}->{"screen_name"};
            delete($vipBCH->{$screen_name});
            if(writeJson(\$vipBCH, "$env_path/vipBCH.json", ">")!=0){
                print "FileWriteError. vipBCH.\n";
                next;
            }
        }elsif(exists $cmd_ref->{"stop"}){
            $isStop = 1;
        }

        unlink $cmdCode;
        if($isStop>0){
            last;
        }
    }

    sleep($cycle_sec);

}

exit 0;

sub writeJson {
       my $hash_ref = shift; #IN
       my $filePath = shift; #IN
       my $mode = shift;

       # save to Json.
       # utf8::encode($$hash_ref);

       open (OUT, $mode, $filePath) || return(1);
       binmode(OUT, ":utf8");
       print OUT to_json($$hash_ref, {pretty=>1});
       close(OUT);
       return (0);
}

sub readJson {
       my $hash_ref = shift; # OUT
       my $filePath = shift; #IN

       %{$$hash_ref} = ();

       open( IN, '<', $filePath) || return(1);
       eval{
              local $/ = undef;
              my $json_text = <IN>;
              close(IN);
              $$hash_ref = decode_json($json_text );
       };
       return (0);
}

sub getDate {
       my $date_ref = shift; # OUT

       my $sec;
       my $min;
       my $hour;
       my $mday;
       my $mon;
       my $year;
       my $wday;
       my $yday;
       my $isdst;

       ($sec,$min,$hour,$mday,$mon,$year,$wday,$yday,$isdst) = localtime(time);
       $$date_ref = sprintf("%04d-%02d-%02d %02d:%02d:%02d",$year+1900,$mon+1,$mday,$hour,$min,$sec);

       return (0);
}

sub convertTimeGMTtoJST{
	my $jst_ref	= shift; # OUT
	my $gmt_str	= shift; #  IN

    
    # $tm =~ /^(\s+) (\s+)-(\d+)T(\d+):(\d+):(\d+)/;
	
	my $tm = $gmt_str;
    # 2017-07-19T01:42:20Z
	$tm =~ /^(\d+)-(\d+)-(\d+)T(\d+):(\d+):(\d+)/;
	my $utm = timegm($6, $5, $4, $3, $2-1, $1);
	$$jst_ref = localtime($utm);
	
	return (0);
	#my $tm = $org_closed_on;
	#print "\t\t\t\t\t tm=$tm\n";
	#$tm =~ /^(\d+)-(\d+)-(\d+)T(\d+):(\d+):(\d+)/;
	#my $utm = timegm($6, $5, $4, $3, $2-1, $1);
	#print "\t\t\t\t\t utm=$utm\n";
	#my $jst = localtime($utm);
	#my $jst_str = strftime("%Y-%m-%d %H:%M:%S",localtime($utm));
	#print "\t\t\t\t\t jst=$jst_str\n";
}

sub convertTimeTZtoJST{
	my $jst_ref	= shift; # OUT
	my $tz_str	= shift; #  IN

    # Date_Init("TZ=JST");

    # Wed Dec 20 06:07:43 +0000 2017
    my $date = ParseDate($tz_str);
    $$jst_ref = UnixDate($date,"%Y/%m/%d %H:%M:%S");
	
	return (0);
}

sub getHttpStrArray{
	my $array_ref	= shift; # OUT
	my $text	    = shift; #  IN
    my $pos         = shift; #  IN

    # http抜き出し
    my $beg_pos = index($text, "http",$pos);
    #print "beg_pos=$beg_pos\n";
    if($beg_pos != -1){
        my $end_space = index($text, " ",$beg_pos+1);
        #print "end_space=$end_space space.\n";

        my $end_big = index($text, "　",$beg_pos+1);
        #print "end_big=$end_big big space.\n";

        my $end_ret = index($text, "\n",$beg_pos+1);
        #print "end_ret=$end_ret return.\n";

        my $end_pos = -1;
        if($end_space!=-1){
            $end_pos = $end_space - 1;
        }
        if($end_big!=-1){
            if($end_pos==-1){
                $end_pos = $end_big - 1;
            }else{
                if($end_pos>$end_big){
                    $end_pos = $end_big - 1;
                }
            }
        }
        if($end_ret!=-1){
            if($end_pos==-1){
                $end_pos = $end_ret - 1;
            }else{
                if($end_pos>$end_ret){
                    $end_pos = $end_ret - 1;
                }
            }
        }
        if($end_pos == -1){
            $end_pos = length($text)-1;
            #print "end_pos=$end_pos end of string.\n";
        }
        #print "!!! end_pos=$end_pos !!!\n";
        if($end_pos != -1){
            my $http_text = substr($text,$beg_pos,($end_pos-$beg_pos)+1);
            push(@{$array_ref}, $http_text );
            #print "push http_text=$http_text\n";
            if($end_pos != length($text)){
                getHttpStrArray($array_ref, $text, $end_pos+1);
            }
        }
    }
}