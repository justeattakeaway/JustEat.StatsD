require 'pathname'
require 'cocaine'

def setup_local_test_site(params={})
	root_checkout = params[:root_checkout] || '.'
	appcmd = File.join 'c:','windows','system32','inetsrv','appcmd.exe'
	simulated_bucket_path = params[:simulated_bucket_path] || File.join('testing','simulated_bucket')
	namespace :bootstrap do
		namespace :local_test_site do
			task :user_data_site do
				Cocaine::CommandLine.new(appcmd, "delete site /site.name:user_data", logger: @log, expected_outcodes: [0,144]).run
				Cocaine::CommandLine.new("#{appcmd}", "add site /name:user_data /bindings:http://user_data:80 /physicalPath:#{File.expand_path(root_checkout).gsub('/','\\')}", logger: @log).run
			end
			task :bucket_site do
				Cocaine::CommandLine.new(appcmd, "delete site /site.name:bucket_site", logger: @log, expected_outcodes: [0,144]).run
				Cocaine::CommandLine.new(appcmd, "add site /name:bucket_site /bindings:http://bucket_site:80 /physicalPath:#{File.expand_path(simulated_bucket_path).gsub('/','\\')}", logger: @log).run
			end
			task :hosts_entries do
				hosts = Pathname.new('c:/windows/system32/drivers/etc/hosts')
				['user_data', 'bucket_site'].each do |site|
					done_already = hosts.read =~ /#{site}/i
					unless done_already
						hosts.open('a') { |f|
							f.puts
							f.puts "127.0.0.1 #{site} #added when installing bootstrapper"
						}
						@log.info 'added hosts file entry for http://#{site}'
					end
				end
			end
			desc 'Set up the local test site to allow Bootstrapper to be functionally-tested'
			task :default => [:hosts_entries, :user_data_site, :bucket_site]
			task :clobber do
				Cocaine::CommandLine.new(appcmd, "delete site /site.name:bucket_site", logger: @log, expected_outcodes: [0,144]).run
				Cocaine::CommandLine.new(appcmd, "delete site /site.name:user_data", logger: @log, expected_outcodes: [0, 144]).run
			end
		end
		task :default => 'local_test_site:default'
		task :clobber => 'local_test_site:clobber'
	end
end
