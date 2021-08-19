
#UUID=cat /proc/sys/kernel/random/uuid
UUID="TODO"
ID=1

while ((1)); do
    echo "{\"id\": \"$ID\", \"data\": $RANDOM}"
    sleep 1
    let ID=ID+1
done
