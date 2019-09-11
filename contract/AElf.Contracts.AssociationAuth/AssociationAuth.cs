using System;
using System.Linq;
using Acs3;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using CreateProposalInput = Acs3.CreateProposalInput;

namespace AElf.Contracts.AssociationAuth
{
    public partial class AssociationAuthContract : AssociationAuthContractContainer.AssociationAuthContractBase
    {
        #region View

        public override Organization GetOrganization(Address address)
        {
            return State.Organisations[address] ?? new Organization();
        }

        public override ProposalOutput GetProposal(Hash proposalId)
        {
            var proposal = State.Proposals[proposalId];
            if (proposal == null)
            {
                return new ProposalOutput();
            }
            var organization = State.Organisations[proposal.OrganizationAddress];
            var readyToRelease = IsReleaseThresholdReached(proposal, organization);

            return new ProposalOutput
            {
                ProposalId = proposalId,
                ContractMethodName = proposal.ContractMethodName,
                ExpiredTime = proposal.ExpiredTime,
                OrganizationAddress = proposal.OrganizationAddress,
                Params = proposal.Params,
                Proposer = proposal.Proposer,
                ToAddress = proposal.ToAddress,
                ToBeReleased = readyToRelease
            };
        }

        #endregion view

        #region Actions

        public override Address CreateOrganization(CreateOrganizationInput input)
        {
            var organizationHash = Hash.FromTwoHashes(Hash.FromMessage(Context.Self), Hash.FromMessage(input));
            var organizationAddress = Context.ConvertVirtualAddressToContractAddress(organizationHash);
            var organization = new Organization
            {
                ReleaseThreshold = input.ReleaseThreshold,
                OrganizationAddress = organizationAddress,
                ProposerThreshold = input.ProposerThreshold,
                OrganizationHash = organizationHash,
                Reviewers = {input.Reviewers}
            };
            Assert(Validate(organization), "Invalid organization.");
            if (State.Organisations[organizationAddress] == null)
            {
                State.Organisations[organizationAddress] = organization;
            }

            return organizationAddress;
        }


        public override Hash CreateProposal(CreateProposalInput input)
        {
            // check authorization of proposer public key
            var organization = State.Organisations[input.OrganizationAddress];
            Assert(organization != null, "No registered organization.");
            AssertSenderIsAuthorizedProposer(organization);
            Hash hash = Hash.FromMessage(input);
            var proposal = new ProposalInfo
            {
                ContractMethodName = input.ContractMethodName,
                ExpiredTime = input.ExpiredTime,
                Params = input.Params,
                ToAddress = input.ToAddress,
                OrganizationAddress = input.OrganizationAddress,
                ProposalId = hash,
                Proposer = Context.Sender
            };
            Assert(Validate(proposal), "Invalid proposal.");
            //TODO: Proposals with the same input is not supported. 
            Assert(State.Proposals[hash] == null, "Proposal already exists.");
            State.Proposals[hash] = proposal;
            return hash;
        }

        public override BoolValue Approve(ApproveInput input)
        {
            var proposal = GetValidProposal(input.ProposalId);
            AssertProposalNotYetApprovedBySender(proposal);
            var organization = GetOrganization(proposal.OrganizationAddress);
            var reviewer = GetReviewerObjectForSender(organization);

            proposal.ApprovedReviewer.Add(Context.Sender);
            proposal.ApprovedWeight += reviewer.Weight;

            State.Proposals[input.ProposalId] = proposal;

            return new BoolValue {Value = true};
        }

        public override Empty Release(Hash proposalId)
        {
            var proposalInfo = State.Proposals[proposalId];
            Assert(proposalInfo != null, "Proposal not found.");
            Assert(Context.Sender.Equals(proposalInfo.Proposer), "Unable to release this proposal.");
            var organization = State.Organisations[proposalInfo.OrganizationAddress];
            Assert(IsReleaseThresholdReached(proposalInfo, organization), "Not approved.");
            Context.SendVirtualInline(organization.OrganizationHash, proposalInfo.ToAddress,
                proposalInfo.ContractMethodName, proposalInfo.Params);
            
            return new Empty();
        }

        #endregion
    }
}