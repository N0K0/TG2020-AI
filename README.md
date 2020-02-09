# The Gathering 2020 AI competition: Gaia Edition

## What are we going to do?

## Intro to the challenges technical aspects



## The protocol

The protocol is based on JSON packets over WebSockets.

This approach was chosen due to the fact that almost all _reasonable_ languages supports these two concepts either natively or via a mature 3rd party package.



### Connecting to the server

#### Using WebSockets

##### Libraries for different languages

###### Python

###### Javascript (TypeSript?)

###### Java

###### C#

###### Others

You can use whatever you want of languages, but these are the ones i'm ready to compile/run.
Contact me if you want to use something else, and 



### State sent to the player

There is two different types of state sent to the use.

The first one is the definition of the game map. As precisely as possible. Since this might be quite big i opt on only sending it a couple of times



#### The game map





##### What is an Bezier curve?





### Commands

In general all commands will return a couple of different answers, all basic commands return an OK or ERROR. Other than that we will return things like a bit more comprehensive status or such.



The two basic returns are `OK` and `ERROR`.

Both are based on the Type field which is the same as the command sent. And a status field with the value  `OK` or `ERROR` on top of that the `Command`  field is used for some debug friendly strings

```json
{
	"Type": "Username",
    "Status": "OK"
	"Command": "Username set!"
}
```

```json
{
	"Type": "Username"
    "Status": "ERROR"
	"Command": "Username already in use, set an other if "

}
```



#### Useful game commands





#### Misc commands

##### Set username

**Note:** This is not optional in this build. If username is not set you _will_ get kicked.

###### Structure

```json
{
	"Type": "Username"
	"Command": "The username i want"
}
```

###### Returns

Generic returns



##### Set color trace:

_At the time of writing I have not decided what to do if someone recalls this command ;)_

###### Structure

```json
{
	"Type": "Color"
	"Command": "00FFA3"
}
```

###### Returns

Generic returns


### Examples

## Prizes

## Rules

### General Rules
### Competition Rules




## Other
Big Thanks to Sebastian Lague for awesome Unity tutorials (Hey! This is my first project)
https://www.patreon.com/SebastianLague



Newtonsoft JSON .Net. Jesus christ, apparently half my computer uses you.