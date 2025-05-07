#!/bin/bash

LOG_FILE="access.log"

echo "=== LOG FILE ANALYSIS ==="

# Total Requests
total_requests=$(wc -l < "$LOG_FILE")
echo "Total number of requests: $total_requests"

# GET and POST counts
get_count=$(grep -o '"GET' "$LOG_FILE" | wc -l)
post_count=$(grep -o '"POST' "$LOG_FILE" | wc -l)
echo "Number of GET requests: $get_count"
echo "Number of POST requests: $post_count"

# Unique IP Addresses
unique_ips=$(awk '{print $1}' "$LOG_FILE" | sort | uniq | wc -l)
echo "Unique IP addresses: $unique_ips"

# GET and POST per IP
echo "GET and POST count per IP:"
awk '
{
    ip[$1]++;
    if ($6 == "GET") get[$1]++;
    if ($6 == "POST") post[$1]++;
}
END {
    for (i in ip) {
        printf "%s: Total=%d, GET=%d, POST=%d\n", i, ip[i], get[i]+0, post[i]+0;
    }
}' "$LOG_FILE"

# Failed Requests (4xx or 5xx)
failed_requests=$(awk '$9 ~ /^[45]/ {count++} END {print count+0}' "$LOG_FILE")
failure_percentage=$(awk -v total="$total_requests" -v failed="$failed_requests" 'BEGIN {printf "%.2f%%", (failed / total) * 100}')
echo "Failed requests (4xx or 5xx): $failed_requests"
echo "Percentage of failed requests: $failure_percentage"

# Most Active IP
most_active_ip=$(awk '{ip[$1]++} END {for (i in ip) print ip[i], i}' "$LOG_FILE" | sort -nr | head -n1)
echo "Most active IP address: $most_active_ip"

# Daily Request Average
daily_requests=$(awk '{date[$4]=$date[$4]+1} END {for (d in date) print d}' "$LOG_FILE" | cut -d: -f1 | sort | uniq -c)
total_days=$(echo "$daily_requests" | wc -l)
average_daily_requests=$(awk -v total="$total_requests" -v days="$total_days" 'BEGIN {printf "%.2f", total / days}')
echo "Average daily requests: $average_daily_requests"

# Day with highest failures
echo "Failures per day:"
awk '$9 ~ /^[45]/ {day=substr($4,1,11); fail[day]++} END {for (d in fail) print fail[d], d}' "$LOG_FILE" | sort -nr

# Hourly Request Count
echo "Requests by hour:"
awk '{hour=substr($4,14,2); hourly[hour]++} END {for (h in hourly) print h, hourly[h]}' "$LOG_FILE" | sort -n

# Status Code Breakdown
echo "Status code breakdown:"
awk '{status[$9]++} END {for (s in status) print s, status[s]}' "$LOG_FILE" | sort -n

# Most Active IP by Method
echo "Top IP using GET:"
awk '$6 == "GET" {ips[$1]++} END {for (ip in ips) print ips[ip], ip}' "$LOG_FILE" | sort -nr | head -n1
echo "Top IP using POST:"
awk '$6 == "POST" {ips[$1]++} END {for (ip in ips) print ips[ip], ip}' "$LOG_FILE" | sort -nr | head -n1

# Patterns in Failures by Hour
echo "Failure requests by hour:"
awk '$9 ~ /^[45]/ {hour=substr($4,14,2); fail_hour[hour]++} END {for (h in fail_hour) print h, fail_hour[h]}' "$LOG_FILE" | sort -n
