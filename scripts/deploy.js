const hre = require("hardhat");
const fs = require("fs");

async function main() {
  const GameCharacterNFT = await hre.ethers.getContractFactory("GameCharacterNFT");
  const gameCharacterNFT = await GameCharacterNFT.deploy();
  await gameCharacterNFT.deployed();
  console.log("GameCharacterNFT deployed to:", gameCharacterNFT.address);

  const GameCharacterMarketplace = await hre.ethers.getContractFactory("GameCharacterMarketplace");
  const gameCharacterMarketplace = await GameCharacterMarketplace.deploy();
  await gameCharacterMarketplace.deployed();
  console.log("GameCharacterMarketplace deployed to:", gameCharacterMarketplace.address);

  const deploymentInfo = {
    "GameCharacterNFT": {
      "address": gameCharacterNFT.address,
      "abi": JSON.parse(gameCharacterNFT.interface.format(hre.ethers.utils.FormatTypes.json))
    },
    "GameCharacterMarketplace": {
      "address": gameCharacterMarketplace.address,
      "abi": JSON.parse(gameCharacterMarketplace.interface.format(hre.ethers.utils.FormatTypes.json))
    }
  }

  fs.writeFileSync("unity/Assets/Resources/deployment-info.json", JSON.stringify(deploymentInfo, null, 2));
  console.log("Deployment info written to unity/Assets/Resources/deployment-info.json");
}

main()
  .then(() => process.exit(0))
  .catch((error) => {
    console.error(error);
    process.exit(1);
  });
