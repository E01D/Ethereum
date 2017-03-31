using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.Geth;
using System.Threading;
using NUnit;
using NUnit.Framework;
using Assert = Xunit.Assert;

namespace NethereumTutorials
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public async Task ShouldBeAbleToDeployAContract()
        {
            //Create a new Variable that will act as the Sender's address
            var senderAddress = "0x12890d2cce102216644c59daE5baed380d84830c";
            //Create a variable that will be the password, for the sake of being easy, I named it password
            var password = "password";
            //This abi variable is very important. After creating the variable, go to the smart contract that was created earlier and copy the entire contents of the .abi file into a word doc and replace all of the " with "".
            var abi = @"[{""constant"":false,""inputs"":[{""name"":""val"",""type"":""int256""}],""name"":""multiply"",""outputs"":[{""name"":""d"",""type"":""int256""}],""payable"":false,""type"":""function""},{""inputs"":[{""name"":""multiplier"",""type"":""int256""}],""payable"":false,""type"":""constructor""}]";
            //The byte code can be found in the smart contract file that was created earlier that ends with .bin. Copy the entire address here and use the prefix 0x
            var byteCode = "0x60606040523415600b57fe5b6040516020806100ac83398101604052515b60008190555b505b6079806100336000396000f300606060405263ffffffff60e060020a6000350416631df4f14481146020575bfe5b3415602757fe5b60306004356042565b60408051918252519081900360200190f35b60005481025b9190505600a165627a7a7230582078a3eded5c478525337867479bd6ffd45fe64ae9969c72144c73a4af136e3c920029";
            //create a new variable that will be mulitplied if the transaction is recieved.
            var multiplier = 7;
            //Sync up with your TestChain
            var web3Geth = new Web3Geth("http://192.168.1.102:8545");
            //Send the pack
            var unlockAccountResult = await web3Geth.Personal.UnlockAccount.SendRequestAsync(senderAddress, password, 120);
            Assert.True(unlockAccountResult);
         
            var transactionHash = await web3Geth.Eth.DeployContract.SendRequestAsync(abi,byteCode, senderAddress,multiplier);
            //Mine the transaction to confirm the transaction was recieved
            var mineResult = await web3Geth.Miner.Start.SendRequestAsync(6);
            Assert.True(mineResult);
            var receipt = await web3Geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            //If the transaction is not recieved, wait 5000 milliseconds (5 Seconds) before 
            while (receipt == null)
            {
                Thread.Sleep(5000);
                receipt = await web3Geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

            }

            var contractAddress = receipt.ContractAddress;
            var contract = web3Geth.Eth.GetContract(abi, contractAddress);
            //after the entire transaction takes place multiply the numbers
            var multiplyFunction = contract.GetFunction("multiply");
            //Numbers will equal 49 if true. 
            var result = await multiplyFunction.CallAsync<int>(7);
            Assert.Equal(49, result);
        }

    }
}
