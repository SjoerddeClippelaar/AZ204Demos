



read prefix

# create resource group
myLocation="westeurope"
myResourceGroup="demos-az204-module13-rg"
az group create -n $myResourceGroup -l $myLocation


# create redis cache
# takes a long time to finish creating but CLI command will return before that
redisname="${prefix}az204redis"
az redis create -l $myLocation -g $myResourceGroup -n $redisname --sku Basic --vm-size c0

rediskey=$(az redis list-keys -g $myResourceGroup -n $redisname --query "primaryKey" -o tsv)
echo "Redis key: ${rediskey}"
echo "StackExchange.Redis connection string: ${redisname}.redis.cache.windows.net:6380,password=${rediskey},ssl=True,abortConnect=False"


# See Module13.Redis.ConsoleApp for basic demo
# copy paste the redis cache name and rediskey to the constant strings in Program.cs

# See Module13.Redis.BlazorApp for more advanced, realistic demo
# Copy the connection string to appsettings.json
#
# BlazorApp demo based on youtube tutorial by Tim Corey:
# https://www.youtube.com/watch?v=UrQWii_kfIE
