﻿using Microsoft.EntityFrameworkCore;
using Rauthor.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Rauthor.ViewModels
{
    public class ProfileModel
    {
        public string Name { get; set; }
        public string Login { get; set; }
        public int Rating { get; set; }
        public bool Verificated { get; set; }
        public string Status { get; set; }
        public string About { get; set; }
        public IEnumerable<Participant> Participants { get; }
        public IEnumerable<Competition> ParticipatedCompetitions { get; }
        public ProfileModel(
            string login,
            int rating,
            bool verificated,
            string name=null,
            string status=null,
            string about=null,
            IEnumerable<Participant> participants=null,
            IEnumerable<Competition> participatedCompetitions=null)
        {
            Login = login;
            Rating = rating;
            Verificated = verificated;
            Name = name;
            Status = status;
            About = about;
            Participants = participants;
            ParticipatedCompetitions = participatedCompetitions;
        }
        public static ProfileModel ReadFromDatabase(Guid userGuid, DatabaseContext database)
        {
            Contract.Assert(database != null);
            var user = database.Users.FirstOrDefault(u => u.Guid == userGuid);
            database.Participants.Where(p => p.UserGuid == userGuid).Load();
            database.Competitions.Where(c => user.Participants.Select(p => p.CompetitionGuid).Contains(c.Guid)).Load();
            return new ProfileModel(
                user.Login,
                rating: user.Participants.Sum(p => p.JuryScore + p.UserScore),
                verificated: false,
                participants: user.Participants.Where(p => !p.Approved),
                participatedCompetitions: database.Participants
                                              .Where(p => p.Approved && p.UserGuid == userGuid)
                                              .Select(p => p.Competition));
        }
    }
}