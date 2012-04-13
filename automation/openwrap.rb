require 'rake'

def windows?
	return :win2003 if ENV['userprofile'] =~ /documents/i
	return :win2008
end

def install_openwrap_to_profile(params={})
	openwrap_in_3rdparty = File.join params[:path_to_3rdparty], 'openwrap'
	@log.info "OpenWrap not found; installing from #{openwrap_in_3rdparty}"
	chdir openwrap_in_3rdparty do
		sh "cmd /c o.exe -shellInstall install" do |ok, result|
			@log.warn "shell-install reported failure with code #{result}" unless ok
		end
		if windows? == :win2003
			sh "set PATH=%PATH%;%USERPROFILE%\\local settings\\application data\\openwrap" unless is_build_agent? and not ENV['path'] =~ /openwrap/i #build agents are treated differently, see http://dhickey.ie/post/2011/04/14/Getting-TeamCity-Build-Agents-and-OpenWrap-to-work-together.aspx
		else
			sh "set PATH=%PATH%;%USERPROFILE%\\appdata\\local\\openwrap" unless is_build_agent? and not ENV['path'] =~ /openwrap/i #build agents are treated differently, see http://dhickey.ie/post/2011/04/14/Getting-TeamCity-Build-Agents-and-OpenWrap-to-work-together.aspx
		end
		sh "cmd /c o.exe add-remote openwrap-beta http://wraps.openwrap.org/beta/" do |ok, result|
			raise "failed to add remote openwrap-beta" unless ok
		end
		@log.info "Updating system openwrap install.  This can take some time... (like, minutes)"
		sh "cmd /c o.exe update-wrap openwrap -system" do |ok, result|
			raise "failed to update system-repository (in your userprofile) openwrap to v2/latest" unless ok
		end
		sh "cmd /c o.exe add-remote openwrap-beta http://wraps.openwrap.org/beta/" do |ok, result|
			raise "failed to re-add remote openwrap-beta" unless ok
		end
		@log.info "Updating wraps.  This can take some time... (like, minutes)"
		sh "cmd /c o.exe update-wrap openwrap -system" do |ok, result|
			raise "failed to update openwrap from our own package to get R# 6.1 integration" unless ok
		end
	end
	@log.happy "You should now have openwrap installed and present in your user-profile."
	@log.happy "In subsequent console windows, you should be able to call it like `o get-help`"
	@log.happy "See http://www.openwrap.org for more information"
end

def setup_openwrap_dependencies(params={})
	path_to_3rdparty = params[:path_to_3rdparty] || '../../../3rdparty'
	path_to_local_wraps = params[:path_to_local_wraps] || 'd:/wraps_local/'
	unc_path_to_continuous_wraps = params[:unc_path_to_continuous_wraps] || '\\\\ci.dev\\continuous\\'
	unc_path_to_third_party_wraps = params[:unc_path_to_third_party_wraps] || '\\\\ci.dev\\thirdparty\\'
	namespace :bootstrap do
		namespace :openwrap do
			desc 'Install openwrap to user-profile and add to path'
			task :install do
				@log.debug "openwrap:install - ENV['USERPROFILE'] #{ENV['userprofile']}"
				path_in_profile = File.join(ENV['userprofile'], 'appdata', 'local', 'openwrap', 'o.exe') if windows? == :win2008
				path_in_profile = File.join(ENV['userprofile'], 'local settings', 'application data', 'openwrap', 'o.exe') if windows? == :win2003
				unless File.exist? path_in_profile
					install_openwrap_to_profile :path_to_3rdparty => path_to_3rdparty
				end
				@o = "\"#{File.expand_path(path_in_profile).gsub('/','\\')}\""
			end
			desc 'Add openwrap remotes if not present'
			task :remotes => [:install] do
				chdir '..' do
					cmd = "cmd /c #{@o} list-remote"
					@log.info "Adding standard remote repositories if required..."
					@log.debug cmd
					remotes = `#{cmd}`
					@log.info remotes
					sh "cmd /c #{@o} add-remote openwrap-beta http://wraps.openwrap.org/beta/" unless remotes =~ /\d+.*openwrap-beta/i
					sh "cmd /c #{@o} add-remote thirdparty file://#{unc_path_to_third_party_wraps.gsub('/','\\')} -priority 10" unless remotes =~ /\d+.*thirdparty/i
					if is_build_agent?
						sh "cmd /c #{@o} add-remote continuous file://#{unc_path_to_continuous_wraps.gsub('/','\\')} -priority 20" unless remotes =~ /\d+.*continuous/i
					end
					unless is_build_agent?
						mkdir_p path_to_local_wraps unless File.exist? path_to_local_wraps
						sh "cmd /c #{@o} add-remote local file://\\#{path_to_local_wraps.gsub('/','\\')} -priority 5" unless remotes =~ /\d+.*local/i
					end
					@log.info "Done."
				end
			end
			anchored_openwrap = File.join 'wraps', 'openwrap'
			file anchored_openwrap => [:remotes] do
				exists = File.exist? anchored_openwrap
				@log.debug "#{anchored_openwrap} exists? #{exists}"
				unless exists
					@log.info "Updating openwrap wrap.  This can take some time (like, several minutes)..."
					sh "cmd /c #{@o} update-wrap openwrap -UseSystem"
				end
			end
			desc 'update openwrap to system-repository version'
			task :wraps => anchored_openwrap
			task :default => [:install, :remotes, :wraps]
		end
		desc 'Bootstrap openwrap'
		task :openwrap => 'openwrap:default'
		task :default => :openwrap
	end
end
