using System;
using System.Security.Claims;
using API.Extensions;
using API.Models;
using API.Services;
using API.Siren;
using API.TransferModels.InputModels;
using FluentSiren.Builders;
using FluentSiren.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace API.TransferModels.ResponseModels
{
    public class TeachersSirenHto : SirenCollectionHto<Teacher>
    {
        
        protected override string Class { get; } = "teacher";
        protected override string RouteList { get; } = Routes.TeacherList;

        public TeachersSirenHto(
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor) :
                base(urlHelperFactory, actionContextAccessor)
        {
        }

        /*
        |-----------------------------------------------------------------------
        | Entity
        |-----------------------------------------------------------------------
        */

        protected override SirenEntityBuilder AddEntityProperties(SirenEntityBuilder entity, Teacher item)
        {
            return entity
                .WithProperty("number", item.Number)
                .WithProperty("name", item.Name)
                .WithProperty("email", item.Email);
        }

        protected override SirenEntityBuilder AddEntitySubEntities(SirenEntityBuilder entity, Teacher item)
        {
            return entity;
        }

        protected override SirenEntityBuilder AddEntityActions(SirenEntityBuilder entity, Teacher item)
        {
            Claim c = Context.HttpContext.User.FindFirst(ClaimTypes.Role);
            if (c != null && c.Value.Equals(Roles.Admin))
            {
                entity
                    .WithAction(
                        new ActionBuilder()
                            .WithName("edit-teacher")
                            .WithTitle("Edit Teacher")
                            .WithMethod("PUT")
                            .WithHref(Url.ToTeacher(Routes.TeacherEdit, item.Number))
                            .WithType("application/json")
                            .WithField(
                                new FieldBuilder()
                                    .WithTitle("Name")
                                    .WithName("name")
                                    .WithType("text")
                                    .WithValue(item.Name))
                            .WithField(
                                new FieldBuilder()
                                    .WithTitle("E-mail")
                                    .WithName("email")
                                    .WithType("email")
                                    .WithValue(item.Email)))
                    .WithAction(
                        new ActionBuilder()
                            .WithName("delete-teacher")
                            .WithTitle("Delete Teacher")
                            .WithMethod("DELETE")
                            .WithHref(Url.ToTeacher(Routes.TeacherDelete, item.Number))
                    );
            }
            this.AddCollectionActions(entity);

            return entity;
        }

        protected override SirenEntityBuilder AddEntityLinks(SirenEntityBuilder entity, Teacher item)
        {
            return entity
                .WithLink(new LinkBuilder()
                    .WithRel("self")
                    .WithHref(Url.ToTeacher(Routes.TeacherEntry, item.Number)))
                .WithLink(new LinkBuilder()
                    .WithRel(SirenData.REL_TEACHERS_CLASSES)
                    .WithHref(Url.ToTeacher(Routes.TeacherClassList, item.Number)))
                .WithLink(new LinkBuilder()
                    .WithRel(SirenData.REL_TEACHER_COURSES)
                    .WithHref(Url.AbsoluteRouteUrl(
                        Routes.TeacherCourseList,
                        new { number = item.Number }))
                );
        }

        /*
        |-----------------------------------------------------------------------
        | Collection
        |-----------------------------------------------------------------------
        */

        protected override SirenEntityBuilder AddCollectionActions(SirenEntityBuilder entity)
        {
            Claim c = Context.HttpContext.User.FindFirst(ClaimTypes.Role);
            if (c != null && c.Value.Equals(Roles.Admin)){
                entity
                    .WithAction(new ActionBuilder()
                        .WithName("add-teacher")
                        .WithTitle("Add Teacher")
                        .WithMethod("POST")
                        .WithHref(UrlTo(Routes.TeacherCreate))
                        .WithType("application/json")
                        .WithField(new FieldBuilder()
                            .WithTitle("Number")
                            .WithName("number")
                            .WithType("number"))
                        .WithField(new FieldBuilder()
                            .WithTitle("Name")
                            .WithName("name")
                            .WithType("text"))
                        .WithField(new FieldBuilder()
                            .WithTitle("E-mail")
                            .WithName("email")
                            .WithType("email"))
                        .WithField(new FieldBuilder()
                            .WithTitle("Password")
                            .WithName("password")
                            .WithType("password"))
                        .WithField(new FieldBuilder()
                            .WithTitle("Admin")
                            .WithName("isAdmin")
                            .WithType("checkbox")));
            }

            return entity;
        }

        protected override SirenEntityBuilder AddCollectionLinks(SirenEntityBuilder entity)
        {
            return entity
                .WithLink(new LinkBuilder()
                    .WithRel("self")
                    .WithHref(UrlTo(Routes.TeacherList)))
                .WithLink(new LinkBuilder()
                    .WithRel("index")
                    .WithHref(UrlTo(Routes.Index)));
        }
    }
}