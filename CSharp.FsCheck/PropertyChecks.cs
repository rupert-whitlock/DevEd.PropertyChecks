using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FsCheck.Fluent;
using FsCheck;
using Microsoft.FSharp.Collections;

namespace CSharp.FsCheck
{
    [TestFixture]
    public class ManualChecks
    {
        [Test]
        public void SanityCheck()
        {
            PhoneNumber ph;
            PhoneNumber.TryParse("+44 123 456789", out ph);
            Assert.AreEqual(ph.CountryCode, 44);
            Assert.AreEqual(ph.IdentificationCode, 123);
            Assert.AreEqual(ph.SubscriberNumber, 456789);
        }
    }


    [TestFixture]
    public class PropertyChecks
    {
        public class GeneratedValidNumber {
            public int Country { get; private set; }
            public int? Identification { get; private set; }
            public int Subscriber { get; private set; }
            public string InputString { get; private set; }

            public GeneratedValidNumber(int country, int? identification, int subscriber)
            {
                Country = country;
                Identification = identification;
                Subscriber = subscriber;
                var idString =
                    identification.HasValue ? " " + identification.ToString() : "";
                InputString = "+" + country.ToString() + idString + " " + subscriber.ToString();
            }

            public override string ToString()
            {
                return "<" + InputString + ">";
            }
        }

        public Gen<GeneratedValidNumber> ValidPhoneNumberGenerator()
        {
            var nullableGen =
                from i in Any.IntBetween(1, 9999)
                select new Nullable<int>(i);
            var numberGen =
                from country in Any.IntBetween(1, 999)
                from identification in Any.GeneratorIn<int?>(nullableGen, Any.Value<int?>(null))
                from subscriber in Any.IntBetween(1, 99999999)
                select new GeneratedValidNumber(country, identification, subscriber);
            return numberGen;
        }

        public Gen<GeneratedValidNumber> InvalidPhoneNumberGenerator()
        {
            var nullableGen =
                from i in Any.IntBetween(10000, 999999999)
                select new Nullable<int>(i);
            var numberGen =
                from country in Any.IntBetween(1000, 999999999)
                from identification in Any.GeneratorIn<int?>(nullableGen, Any.Value<int?>(null))
                from subscriber in Any.IntBetween(10000000, 999999999)
                select new GeneratedValidNumber(country, identification, subscriber);
            return numberGen;
        }

        [Test]
        public void SubscriptionNumberLessThan15Characters()
        {
            Spec.ForAny(
                (DontSize<UInt32> subscriptionNumber) =>
                {
                    var sn = subscriptionNumber.Item;
                    PhoneNumber ph;
                    var ec = PhoneNumber.TryParse("+1 " + sn.ToString(), out ph);
                    return ph.SubscriberNumber.ToString().Length <= 14;
                })
                .QuickCheckThrowOnFailure();
        }

        [Test]
        public void hjghg()
        {
            Spec.ForAny(
                (DontSize<UInt32> subscriptionNumber) =>
                {
                    var sn = subscriptionNumber.Item;
                    PhoneNumber ph;
                    var ec = PhoneNumber.TryParse("+12 " + sn.ToString(), out ph);
                    return ph.SubscriberNumber.ToString().Length <= 13;
                })
                .QuickCheckThrowOnFailure();
        }


        [Test]
        public void IdentificationLessThan4digits()
        {
            Spec.ForAny(
                (DontSize<UInt16> identificationCode) =>
                {
                    var ic = identificationCode.Item;
                    PhoneNumber ph;
                    var ec = PhoneNumber.TryParse("+1 " + ic.ToString() + " 123456", out ph);
                    return ph.IdentificationCode < 10000;
                })
                .QuickCheckThrowOnFailure();
        }

        [Test]
        public void CountryCodeLessThan4digits()
        {
            Spec.ForAny(
                (DontSize<UInt16> country) =>
                {
                    var cc = country.Item;
                    PhoneNumber ph;
                    var ec = PhoneNumber.TryParse("+" + cc.ToString() + " 1234 123456", out ph);
                    return ph.CountryCode < 1000;
                })
                .QuickCheckThrowOnFailure();
        }

        [Test]
        public void ValidNumbersAreRecognized()
        {
            Spec.For(ValidPhoneNumberGenerator(),
                (GeneratedValidNumber n) => {
                    PhoneNumber ph;
                    return PhoneNumber.TryParse(n.InputString, out ph);
                })
                .QuickCheckThrowOnFailure();
        }

        [Test]
        public void VValidNumbersAreRecognized()
        {
            Spec.For(ValidPhoneNumberGenerator(),
                (GeneratedValidNumber n) =>
                {
                    var cc = n.Country.ToString().Length;
                    var ic = n.Identification.ToString().Length;
                    var expectedLength = (15 - cc - ic);

                    PhoneNumber ph;
                    PhoneNumber.TryParse(n.InputString, out ph);

                    return ph.SubscriberNumber.ToString().Length <= expectedLength;
                })
                .QuickCheckThrowOnFailure();
        }



        [Test]
        public void unhappyVValidNumbersAreRecognized()
        {
            Spec.For(InvalidPhoneNumberGenerator(),
                (n) =>
                {
                    PhoneNumber ph;
                    return !PhoneNumber.TryParse(n.InputString, out ph);
                })
                .QuickCheckThrowOnFailure();
        }
    
    }
}