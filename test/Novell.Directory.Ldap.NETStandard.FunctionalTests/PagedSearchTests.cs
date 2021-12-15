﻿using Novell.Directory.Ldap.Controls;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Novell.Directory.Ldap.SearchExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class PagedSearchTests : IClassFixture<PagedSearchTests.PagedSearchTestsAsyncFixture>
    {
        private readonly PagedSearchTestsAsyncFixture _pagedSearchTestsFixture;
        private readonly SearchOptions _searchOptions;
        private readonly LdapSortControl _ldapSortControl;
        private readonly SearchOptions _searchOptionsForZeroResults;

        public PagedSearchTests(PagedSearchTestsAsyncFixture pagedSearchTestsFixture)
        {
            _pagedSearchTestsFixture = pagedSearchTestsFixture;
            _searchOptions = new SearchOptions(
                TestsConfig.LdapServer.BaseDn,
                LdapConnection.ScopeSub,
                "cn=" + _pagedSearchTestsFixture.CnPrefix + "*",
                null);
            _ldapSortControl = new LdapSortControl(new LdapSortKey("cn"), true);
            _searchOptionsForZeroResults = new SearchOptions(
                TestsConfig.LdapServer.BaseDn,
                LdapConnection.ScopeSub,
                "cn=blah*",
                null);
        }

        [Fact(Skip = "Until configured for openldap")]
        [LongRunning]
        public async Task Search_when_paging_using_VirtualListViewControl_returns_expected_results()
        {
            var entries = new List<LdapEntry>();
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    entries = await ldapConnection.SearchUsingVlvAsync(
                        _ldapSortControl,
                        _searchOptions,
                        _pagedSearchTestsFixture.PageSize
                    );
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries);
        }

        [Fact(Skip = "Until configured for openldap")]
        [LongRunning]
        public async Task Search_when_paging_using_VirtualListViewControl_using_converter_returns_expected_results()
        {
            var entries = new List<Tuple<LdapEntry>>();
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    entries = await ldapConnection.SearchUsingVlvAsync(
                        _ldapSortControl,
                        entry => new Tuple<LdapEntry>(entry),
                        _searchOptions,
                        _pagedSearchTestsFixture.PageSize
                    );
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries.Select(x => x.Item1).ToList());
        }

        [Fact(Skip = "Until configured for openldap")]
        [LongRunning]
        public async Task Search_when_paging_using_VirtualListViewControl_with_one_page_returns_expected_results()
        {
            var entries = new List<LdapEntry>();
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    entries = await ldapConnection.SearchUsingVlvAsync(
                        _ldapSortControl,
                        _searchOptions,
                        _pagedSearchTestsFixture.Entries.Count
                    );
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries);
        }

        [Fact(Skip = "Until configured for openldap")]
        [LongRunning]
        public async Task Search_when_paging_using_VirtualListViewControl_returns_zero_results()
        {
            var entries = new List<LdapEntry>();
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    entries = await ldapConnection.SearchUsingVlvAsync(
                        _ldapSortControl,
                        _searchOptionsForZeroResults,
                        _pagedSearchTestsFixture.PageSize
                    );
                });

            Assert.Empty(entries);
        }

        [Fact]
        [LongRunning]
        public async Task Search_when_paging_using_SimplePagedResultControl_returns_expected_results()
        {
            var entries = new List<LdapEntry>();
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    entries.AddRange(
                        await ldapConnection.SearchUsingSimplePagingAsync(
                            _searchOptions,
                            _pagedSearchTestsFixture.PageSize
                        ));
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries);
        }

        [Fact]
        [LongRunning]
        public async Task Search_when_paging_using_SimplePagedResultControl_using_converter_returns_expected_results()
        {
            var entries = new List<Tuple<LdapEntry>>();
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    entries.AddRange(
                        await ldapConnection.SearchUsingSimplePagingAsync(
                            entry => new Tuple<LdapEntry>(entry),
                            _searchOptions,
                            _pagedSearchTestsFixture.PageSize
                        ));
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries.Select(x => x.Item1).ToList());
        }

        [Fact]
        [LongRunning]
        public async Task Search_when_paging_using_SimplePagedResultControl_in_just_one_page_returns_expected_results()
        {
            var entries = new List<LdapEntry>();
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    entries.AddRange(
                        await ldapConnection.SearchUsingSimplePagingAsync(
                            _searchOptions,
                            _pagedSearchTestsFixture.Entries.Count
                        ));
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries);
        }

        [Fact]
        [LongRunning]
        public async Task Search_when_paging_using_SimplePagedResultControl_returns_zero_results()
        {
            var entries = new List<LdapEntry>();
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    entries.AddRange(
                        await ldapConnection.SearchUsingSimplePagingAsync(
                            _searchOptionsForZeroResults,
                            _pagedSearchTestsFixture.Pages
                        ));
                });

            Assert.Empty(entries);
        }

        [Fact]
        [LongRunning]
        public async Task Search_when_paging_using_SimplePagedResultControl_and_large_result_set_returns_expected_results()
        {
            const int PageSize = 40;
            const int Pages = 50;
            await using var largePagedSearchTest = PagedSearchTestsAsyncFixture.Create(Pages, PageSize);
            await largePagedSearchTest.InitializeAsync();
            var searchOptions = new SearchOptions(
                TestsConfig.LdapServer.BaseDn,
                LdapConnection.ScopeSub,
                "cn=" + largePagedSearchTest.CnPrefix + "*",
                null);
            var entries = new List<LdapEntry>();
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    entries.AddRange(
                        await ldapConnection.SearchUsingSimplePagingAsync(
                            searchOptions,
                            largePagedSearchTest.PageSize
                        ));
                });

            Assert.True(entries.Count >= Pages * PageSize);
            AssertReceivedExpectedResults(largePagedSearchTest.Entries, entries);
        }

        private void AssertReceivedExpectedResults(IReadOnlyCollection<LdapEntry> expectedEntries, List<LdapEntry> entries)
        {
            Assert.Equal(expectedEntries.Count, entries.Count);
            foreach (var pair in expectedEntries.OrderBy(x => x.Dn).Zip(entries.OrderBy(x => x.Dn)))
            {
                pair.First.AssertSameAs(pair.Second);
            }
        }

        public sealed class PagedSearchTestsAsyncFixture : IAsyncLifetime
        {
            public int Pages { get; }
            public int PageSize { get; }
            private readonly Random _random = new Random();
            public string CnPrefix { get; }
            public IReadOnlyCollection<LdapEntry> Entries => _entriesTask.Result;

            private Task<LdapEntry[]> _entriesTask;

            public PagedSearchTestsAsyncFixture()
                : this(15, 20)
            {
            }

            public static PagedSearchTestsAsyncFixture Create(int pages, int pageSize)
            {
                return new PagedSearchTestsAsyncFixture(pages, pageSize);
            }

            private PagedSearchTestsAsyncFixture(int pages, int pageSize)
            {
                Pages = pages;
                PageSize = pageSize;
                CnPrefix = Guid.NewGuid().ToString();
            }

            public Task InitializeAsync()
            {
                Console.WriteLine("Create " + (Pages * PageSize) + " entries");
                _entriesTask = Task.WhenAll(
                    Enumerable.Range(1, (Pages * PageSize) + (_random.Next() % PageSize))
                        .Select(x => LdapOps.AddEntryAsync(CnPrefix)));
                return _entriesTask;
            }

            public Task DisposeAsync()
            {
                return Task.CompletedTask;
            }
        }
    }
}
