# FrostySaveManager

An android save manager that provides the functionality of saving the data both locally as well as on PlayServices cloud. It keeps the save files in sync by recording the total time played on a certain save file and always selecting the latest file either from the cloud or from the local disk. The player will always get the latest save files even if he switches his device as long as he logs in with the same google account. Requires no internet connection for offline saves.

## Getting Started

These instructions will get you started in integrating the save manager in your project.

### Prerequisites

An android project with play services enabled and save games setup from the google play console.
Message Packer which is a powerful binary serializer by neuecc which you can download from here https://github.com/neuecc/MessagePack-CSharp. Note: You can use the default unity binary formatter, however you will need to modify the code a little.

### Installing

Installing the save manager is a matter of cloning the repo and pasting it inside your asset folder.
Make sure to install the Message Packer too unless you want to modify the code and use some other serializer.
Remeber to initizlie the play services with .EnableSavedGames() ForExample:

   PlayGamesClientConfiguration config = new
            PlayGamesClientConfiguration.Builder()
            .EnableSavedGames()
            .Build();
            
## How to use FrostySaveManager
# Writing a save game
For writing a save file just create an instance of the FrostWriter<T> class with the T being the class you want to serialize. Then call the save method along with your preferred inputs.
  ForExample: FrostWrite<PlayerLocalSave> _FrostWrite = new FrostWrite<PlayerLocalSave>();
                _FrostWrite.SaveTheGame(instance_to_serialize, SaveFileName);
  
 # Reading a saved game
 For reading a save file just create an instance of the FrostRead<T> class with the T being the class you want to deserialize. Then call the read method along with your preferred inputs.
  ForExample:  FrostRead<PlayerLocalSave> _FrostRead = new FrostRead<PlayerLocalSave>();
               _FrostRead.LoadTheGame(SaveFileName);
  
  # Comparing Local And Cloud Saves
  

## Built With

* [Dropwizard](http://www.dropwizard.io/1.0.2/docs/) - The web framework used
* [Maven](https://maven.apache.org/) - Dependency Management
* [ROME](https://rometools.github.io/rome/) - Used to generate RSS Feeds

## Authors

* **Talha Hanif** - *Initial work* - (https://github.com/mobile94ever)

## Acknowledgments

* Hat tip to anyone whose code was used
