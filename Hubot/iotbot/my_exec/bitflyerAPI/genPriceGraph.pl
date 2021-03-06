#!/usr/bin/perl

use strict;

use warnings;
use Getopt::Std;
use LWP::UserAgent;
use JSON;

use Time::Local;
use Time::Piece;

use GD::Graph::mixed;
use GD::Graph::colour qw( :files );
use GD::Text;

use List::Util qw(max min);

use Fcntl;

my $pricePath = shift;
my $graphPath = shift;
my $sampling_num = shift;

my $key_date;
getDate(\$key_date);
print "date is " . $key_date . "\n";

my $priceHash_ref;
%{$priceHash_ref} = ();
if(-e $pricePath){
	# pricePath-file is exist.

	# Load from Json.
	if( readJson(\$priceHash_ref, $pricePath)==0){
			# print "FileOpenSuccess. $pricePath\n";
	}else{
			print "FileOpenError. $pricePath\n";
			exit -1;
	}

	# Display Keys.
	my @price_keys = keys( %{$priceHash_ref} );
	@price_keys = sort { $a cmp $b } @price_keys;
	my $price_num = @price_keys;
	my @price_values = ();
	for(my $i=0; $i<@price_keys; $i++){
		# print "date[$i]:$price_keys[$i] price:$priceHash_ref->{$price_keys[$i]}\n";
		push(@price_values, $priceHash_ref->{$price_keys[$i]});
	}

	print "generate Graph. price_num=$price_num\n";
	if($price_num<$sampling_num){
		$sampling_num = $price_num;
	}
    my $end = ($price_num-1);
    my $beg = $end - ($sampling_num-1);

    my @price   = @price_values[$beg..$end];
    my @xLabels = ();
    my @moveAvgs = ();

    for(my $i=0; $i<@price; $i++){
            
            push(@xLabels, -((@price-1)-$i));
            # print "price[$i]: $price[$i] $xLabels[$i]\n";
            my $avg = $price[$i];
            if( ($i>0) && ($i<(@price-1))){
                $avg = ($price[$i-1] + $price[$i] + $price[$i+1]) / 3;
            }
            push(@moveAvgs, $avg);
    }

	my @data = ( \@xLabels, \@price);
	# my @data = ( \@xLabels, \@moveAvgs, \@price );
	my $y_min_value = min(@price);
	my $y_max_value = max(@price);

	# print "Y_MIN:$y_min_value\[BTC/JPY\] Y_MAX:$y_max_value\[BTC/JPY\]\n";
	my $graph = GD::Graph::mixed->new( 600, 400 );
	$graph->set( title           => $key_date,
				t_margin         => 10,
				b_margin         => 10,
				l_margin         => 10,
				r_margin         => 10,
				x_label          => "in $sampling_num [minute]",
				x_label_position => 0.5,
				y_label          => "[BTC/JPY]",
				# types            => [ qw(linespoints linespoints) ],
				types            => [ qw(linespoints) ],
				y_max_value      => int($y_max_value/1000) * 1000 + 10000,
				y_min_value      => int($y_min_value/1000) * 1000 - 10000,
				y_tick_number    => 10,
				y_label_skip     => 1,
				line_width       => 2,
				long_ticks       => 10000,
				bar_width        => 10,
				bar_spacing      => 3,
				markers          => [ 5, 5 ],
				transparent      => 0,
				legend_placement => "RT");

	# $graph->set_legend( "[BTC/JPY]");

	my $image = $graph->plot( \@data ) or die( "Cannot create image" );

	# my $graphPath = '.\priceGraph.png';
	open( OUT, '>',$graphPath) or die( "Cannot open filepath:$graphPath $!" );

	binmode OUT;
	print OUT $image->png();
	close OUT;

}

exit 0;

sub writeJson {
       my $hash_ref = shift; #IN
       my $filePath = shift; #IN
       my $mode = shift;

       # save to Json.
       open (OUT, $mode, $filePath) || return(1);
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
